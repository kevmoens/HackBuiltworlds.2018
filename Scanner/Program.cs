using System;
using System.Collections.Generic;
using Windows.UI.ViewManagement;
using Urho;
using Urho.Actions;
using Urho.HoloLens;
using Urho.Shapes;
using Urho.Gui;
using System.Threading.Tasks;
using Shared;



namespace SmartHome.HoloLens
{
	internal class Program
    {
        public static ScannerApp UrhoApp;
        public static MainPage XamlPage;
        [MTAThread]
	    static void Main()
	    {
            // Start XAML app instead of urho app
            // Main is called because we defined DISABLE_XAML_GENERATED_MAIN in the DefineConstants node of the project file.   
            // See http://blog.infernored.com/mixing-hololens-2d-and-3d-xaml-views-in-holographicspace for details

            global::Windows.UI.Xaml.Application.Start((p) => new SmartHome.HoloLens.App());
	    }
	}


    public class ScannerApp : HoloApplication
    {
        public Node lastNode;
        private bool _ShowElectrical = true;
        public bool ShowElectrical { get { return _ShowElectrical; }
            set {
                _ShowElectrical = value;
                try
                {
                    float size;
                    if (_ShowElectrical)
                    {
                        size = 0.1f;
                    }
                    else
                    {
                        size = 0f;
                    }
                    foreach (Node item in ElectricalBaseNode.Children)
                    {
                        item.SetScale(size);
                    }
                }
                catch { }
            }
        }

        private bool _ShowPlumbing = true;
        public bool ShowPlumbing {
            get { return _ShowPlumbing; }
            set {
                _ShowPlumbing = value;
                try
                {
                    float size;
                    if (_ShowPlumbing)
                    {
                        size = .1f;
                    }
                    else
                    {
                        size = 0f;
                    }
                    foreach (Node item in PlumbingBaseNode.Children)
                    {
                        item.SetScale(size);
                    }
                } catch { }
            }
        }
        Node ElectricalBaseNode;
        Node PlumbingBaseNode;
        Node menuNode;
        Node outletNode;
        Node heaterNode;
        Node heaterNodeSave;
        Node environmentNode;
        static bool IsXRay = true;
        SpatialCursor cursor;
        Material material;
        Guid SessionID;
        bool IsPrimary; //Is Primary Session 
        string _surfaceID;
        bool _Debug;
        System.Threading.Timer timer;
        System.Collections.Concurrent.ConcurrentDictionary<string, BulbAddedDto> ExistingBulbs;

        public ScannerApp(ApplicationOptions opts) : base(opts)
        {
            Program.UrhoApp = this;
        }


        protected override async void Start()
        {
            base.Start();

            ExistingBulbs = new System.Collections.Concurrent.ConcurrentDictionary<string, BulbAddedDto>();

            Zone.AmbientColor = new Color(0.3f, 0.3f, 0.3f);
            DirectionalLight.Brightness = 0.5f;

            environmentNode = Scene.CreateChild();
            EnableGestureTapped = true;

            //material = Material.FromColor(Color.Gray); //-- debug mode  //SEE SPATIAL MAPPING
            _Debug = false;
            material = Material.FromColor(Color.Transparent, true);
            //material.SetTechnique(0, CoreAssets.Techniques.NoTextureUnlitVCol, 1, 1);  //UNCOMMENT TO DISABLE SEEING THROUGH WALLS


            ElectricalBaseNode = Scene.CreateChild("ELECTRICAL");
            PlumbingBaseNode = Scene.CreateChild("PLUMBING");

            Action placeOutlet = async delegate ()
            {
                var outletBase = ElectricalBaseNode.CreateChild("OUTLET");
                outletBase.Scale = new Vector3(1, 1f, 1) / 10;
                outletBase.Position = cursor.CursorNode.WorldPosition;
                outletBase.SetDirection(cursor.CursorNode.WorldDirection);


                var nodeOutlet = outletBase.CreateChild();
                //nodeOutlet.Rotate(new Quaternion(0, 90, 90, 90), TransformSpace.Local);


                nodeOutlet.Rotation = new Quaternion(0, 270, -90); // cursor.CursorNode.Rotation.ToEulerAngles().Y, 0);
                //nodeOutlet.RotateAround(new Vector3(0, 0, 0), new Quaternion(0, 270, 90), TransformSpace.Local); //KMM
                lastNode = nodeOutlet;

                nodeOutlet.Position = new Vector3(0, 0, -.25f);
                nodeOutlet.SetScale(.5f);
                //var outletBox = nodeOutlet.CreateComponent<Box>();
                //var material = Material.FromColor(Color.Red, true);
                //outletBox.SetMaterial(material);


                var modelOutlet = nodeOutlet.CreateComponent<StaticModel>();
                modelOutlet.Model = ResourceCache.GetModel("Data\\outlet.mdl");


                //Push to UWP
                var textNode = outletBase.CreateChild("Text");
                var text = textNode.CreateComponent<Text3D>();
                BulbAddedDto bulb = new BulbAddedDto { scale_factor = 0, obj_name = "outlet", Text = "", ID = outletBase.Name, Position = new Vector3Dto(cursor.CursorNode.WorldPosition.X, cursor.CursorNode.WorldPosition.Y, cursor.CursorNode.WorldPosition.Z), Direction = new Vector3Dto(LeftCamera.Node.WorldDirection.X, LeftCamera.Node.WorldDirection.Y, LeftCamera.Node.WorldDirection.Z) };

                ExistingBulbs.TryAdd(bulb.ID, bulb);
                //Shared.SmartHomeService.SmartHomeService proxy = new Shared.SmartHomeService.SmartHomeService();
                //await proxy.AddNote(bulb);

                //SAVE
                outletNode = outletBase;
            };

            Action RemoveOutlet = delegate ()
            {
                if (lastNode != null)
                {
                    try
                    {
                        lastNode.Remove();
                        lastNode.Dispose();
                        lastNode = null;
                    }
                    catch { }
                }
            };
            await RegisterCortanaCommands(new Dictionary<string, Action>() {
                { "place outlet", PlaceOutletModel}
                , {"remove", RemoveOutlet}
                , {"place plumbing", PlacePipeModel}
                , {"X RAY", () =>
                {

                    if (material.Name.Contains("DEBUG"))
                    {
                        material = Material.FromColor(Color.Blue, true);
                        Color startColor = Color.Blue;
                        Color endColor = new Color(0.8f, 0.8f, 0.8f);
                        material.FillMode = FillMode.Wireframe; // wireframe ? FillMode.Wireframe : FillMode.Solid;
                        var specColorAnimation = new ValueAnimation();
                        specColorAnimation.SetKeyFrame(0.0f, startColor);
                        specColorAnimation.SetKeyFrame(1.5f, endColor);
                        material.SetShaderParameterAnimation("MatDiffColor", specColorAnimation, WrapMode.Once, 1.0f);
                    } else
                    {
                        material = Material.FromColor(Color.Transparent, true);
                    }


                    if (IsXRay)
                    {  //TOGGLE XRAY

                        material.SetTechnique(0, CoreAssets.Techniques.NoTextureUnlitVCol, 1, 1);  //UNCOMMENT TO DISABLE SEEING THROUGH WALLS
                        IsXRay = false;
                    } else
                    {
                        material.Name = material.Name + "XRAY";
                        IsXRay = true;
                    }
                    Task.Run(() => {
                        foreach (Node surface in environmentNode.Children)
                        {
                            surface.GetComponent<StaticModel>().SetMaterial(material);
                        }
                        });
                    }
                }
                , {"DEBUG", () => {
                    if (material.Name.Contains("DEBUG"))
                    { //TOGGLE DEBUG
                        material = Material.FromColor(Color.Transparent, true);
                        material.Name = material.Name.Replace("DEBUG", "");
                    } else
                    {
                        material = Material.FromColor(Color.Blue, true);
                        material.Name = material.Name + "DEBUG";

                        Color startColor = Color.Blue;
                        Color endColor = new Color(0.8f, 0.8f, 0.8f);
                        material.FillMode = FillMode.Wireframe; // wireframe ? FillMode.Wireframe : FillMode.Solid;
                        var specColorAnimation = new ValueAnimation();
                        specColorAnimation.SetKeyFrame(0.0f, startColor);
                        specColorAnimation.SetKeyFrame(1.5f, endColor);
                        material.SetShaderParameterAnimation("MatDiffColor", specColorAnimation, WrapMode.Once, 1.0f);
                    }

                    if (IsXRay)
                    {
                        material.SetTechnique(0, CoreAssets.Techniques.NoTextureUnlitVCol, 1, 1);  //UNCOMMENT TO DISABLE SEEING THROUGH WALLS
                    }
                    Task.Run(() => {
                        foreach (Node surface in environmentNode.Children)
                        {
                            surface.GetComponent<StaticModel>().SetMaterial(material);
                        }
                        });
                    }
                }        
				, {"back", ShowXaml }        
            });

            //while (!await ConnectAsync()) { }



            SessionID = Guid.NewGuid();
            IsPrimary = true; // session.IsPrimary;
            await environmentNode.RunActionsAsync(new DelayTime(2));
            await StartSpatialMapping(new Vector3(100, 100, 100));
            InvokeOnMain(() =>
            {
                cursor = Scene.CreateComponent<SpatialCursor>();
            });

            //timer = new System.Threading.Timer(new System.Threading.TimerCallback(CheckStatus), null, 5000, 5000);

        }

        private async void ShowXaml()
        {
            await ApplicationViewSwitcher.SwitchAsync(App.ViewXaml.Id);
        }

        bool Raycast(float maxDistance, out Vector3 hitPos, out Drawable hitDrawable)
        {
            hitDrawable = null;
            hitPos = Vector3.Zero;

            var graphics = Graphics;
            var ui = UI;

            IntVector2 pos = ui.CursorPosition;


            Ray cameraRay = LeftCamera.GetScreenRay(0.5f, .5f);
            var result = Scene.GetComponent<Octree>().RaycastSingle(cameraRay, RayQueryLevel.Triangle, 100, DrawableFlags.Geometry, 0x70000000);
            if (result != null)
            {
                hitPos = result.Value.Position;
                hitDrawable = result.Value.Drawable;
                return true;
            }
            return false;
        }


        void PlaceOutletDecal()
        {


            Vector3 hitPos;
            Drawable hitDrawable;

            if (Raycast(250.0f, out hitPos, out hitDrawable))
            {
                var targetNode = hitDrawable.Node;
                var decal = targetNode.GetComponent<DecalSet>();

                if (decal == null)
                {
                    var cache = ResourceCache;
                    decal = targetNode.CreateComponent<DecalSet>();

                    var i = ResourceCache.GetImage("Data\\sa_control-panel.png");
                    decal.Material = Material.FromImage(i);
                    decal.Material.CullMode = CullMode.Ccw;
                    decal.Material.ShadowCullMode = CullMode.Ccw;
                    decal.Material.FillMode = FillMode.Solid;
                    decal.Material.DepthBias = new BiasParameters(7685, 0);
                    decal.Material.RenderOrder = 128;

                    //decal.Material = cache.GetMaterial("Materials/UrhoDecal.xml");
                }

                // Add a square decal to the decal set using the geometry of the drawable that was hit, orient it to face the camera,
                // use full texture UV's (0,0) to (1,1). Note that if we create several decals to a large object (such as the ground
                // plane) over a large area using just one DecalSet component, the decals will all be culled as one unit. If that is
                // undesirable, it may be necessary to create more than one DecalSet based on the distance
                decal.AddDecal(hitDrawable, hitPos, RightCamera.Node.Rotation, 0.5f, 1.0f, 1.0f, Vector2.Zero,
                    Vector2.One, 0.0f, 0.1f, uint.MaxValue);
            }
        }



        async void PlaceOutletModel()
        {


            Ray cameraRay = LeftCamera.GetScreenRay(0.5f, .5f);
            var result = Scene.GetComponent<Octree>().RaycastSingle(cameraRay, RayQueryLevel.Triangle, 100, DrawableFlags.Geometry, 0x70000000);
            if (result != null)
            {

                var outletBase = ElectricalBaseNode.CreateChild("OUTLET");
                outletBase.Scale = new Vector3(1, 1f, 1) / 10;
                outletBase.Position = result.Value.Position;

                var nodeOutlet = outletBase.CreateChild();

                if (result.Value.Normal != Vector3.Zero)
                {
                    if (result.Value.Normal.X != 0)
                    {
                        nodeOutlet.Rotation = Quaternion.FromRotationTo(Vector3.Back, result.Value.Normal);
                    }
                    else
                    {
                        nodeOutlet.Rotation = Quaternion.FromRotationTo(Vector3.Right, result.Value.Normal);
                    }
                }
                //var neg = result.Value.Node.Rotation;
                //nodeOutlet.Rotation = new Quaternion(neg.X, neg.Y, neg.Z, neg.W * -1); //Opposite of the rotation of the Node, if we use result.Value.Position


                nodeOutlet.Position -= (result.Value.Normal * 0.25f);

                nodeOutlet.SetScale(.5f);
                lastNode = nodeOutlet;

                var modelOutlet = nodeOutlet.CreateComponent<StaticModel>();
                modelOutlet.Model = ResourceCache.GetModel("Data\\outlet.mdl");


                //Push to UWP
                var textNode = outletBase.CreateChild("Text");
                var text = textNode.CreateComponent<Text3D>();
                BulbAddedDto bulb = new BulbAddedDto { scale_factor = 0, obj_name = "outlet", Text = "", ID = outletBase.Name, Position = new Vector3Dto(cursor.CursorNode.WorldPosition.X, cursor.CursorNode.WorldPosition.Y, cursor.CursorNode.WorldPosition.Z), Direction = new Vector3Dto(LeftCamera.Node.WorldDirection.X, LeftCamera.Node.WorldDirection.Y, LeftCamera.Node.WorldDirection.Z) };

                ExistingBulbs.TryAdd(bulb.ID, bulb);
                //SAVE
                outletNode = outletBase;
            }
        }


        async void PlaceOutletModelParallelToGround()
        {


            Ray cameraRay = LeftCamera.GetScreenRay(0.5f, .5f);
            var result = Scene.GetComponent<Octree>().RaycastSingle(cameraRay, RayQueryLevel.Triangle, 100, DrawableFlags.Geometry, 0x70000000);
            if (result != null)
            {

                var outletBase = ElectricalBaseNode.CreateChild("OUTLET");
                outletBase.Scale = new Vector3(1, 1f, 1) / 10;
                outletBase.Position = result.Value.Position;

                var nodeOutlet = outletBase.CreateChild();

                if (result.Value.Normal != Vector3.Zero)
                {
                    if (result.Value.Normal.X != 0)
                    {
                        nodeOutlet.Rotation = Quaternion.FromRotationTo(Vector3.Back, result.Value.Normal);
                    }
                    else
                    {
                        nodeOutlet.Rotation = Quaternion.FromRotationTo(Vector3.Right, result.Value.Normal);
                    }
                }
                //var neg = result.Value.Node.Rotation;
                //nodeOutlet.Rotation = new Quaternion(neg.X, neg.Y, neg.Z, neg.W * -1); //Opposite of the rotation of the Node, if we use result.Value.Position


                nodeOutlet.Position += (result.Value.Normal * 0.25f);

                nodeOutlet.SetScale(.5f);
                lastNode = nodeOutlet;

                var modelOutlet = nodeOutlet.CreateComponent<StaticModel>();
                modelOutlet.Model = ResourceCache.GetModel("Data\\cube.mdl");


                //Push to UWP
                var textNode = outletBase.CreateChild("Text");
                var text = textNode.CreateComponent<Text3D>();
                BulbAddedDto bulb = new BulbAddedDto { scale_factor = 0, obj_name = "outlet", Text = "", ID = outletBase.Name, Position = new Vector3Dto(cursor.CursorNode.WorldPosition.X, cursor.CursorNode.WorldPosition.Y, cursor.CursorNode.WorldPosition.Z), Direction = new Vector3Dto(LeftCamera.Node.WorldDirection.X, LeftCamera.Node.WorldDirection.Y, LeftCamera.Node.WorldDirection.Z) };

                ExistingBulbs.TryAdd(bulb.ID, bulb);
                //SAVE
                outletNode = outletBase;
            }
        }




        async void PlacePipeModel()
        {


            Ray cameraRay = LeftCamera.GetScreenRay(0.5f, .5f);
            var result = Scene.GetComponent<Octree>().RaycastSingle(cameraRay, RayQueryLevel.Triangle, 100, DrawableFlags.Geometry, 0x70000000);
            if (result != null)
            {

                var outletBase = PlumbingBaseNode.CreateChild("PIPE");
                outletBase.SetScale(0.01f);
                outletBase.Position = result.Value.Position;

                var nodeOutlet = outletBase.CreateChild();

                //if (result.Value.Normal != Vector3.Zero)
                //{
                //    if (result.Value.Normal.X != 0)
                //    {
                //        nodeOutlet.Rotation = Quaternion.FromRotationTo(Vector3.Back, result.Value.Normal);
                //    }
                //    else
                //    {
                //        nodeOutlet.Rotation = Quaternion.FromRotationTo(Vector3.Right, result.Value.Normal);
                //    }
                //}
                //var neg = result.Value.Node.Rotation;
                //nodeOutlet.Rotation = new Quaternion(neg.X, neg.Y, neg.Z, neg.W * -1); //Opposite of the rotation of the Node, if we use result.Value.Position


                nodeOutlet.Position -= (result.Value.Normal * 0.25f);

                nodeOutlet.SetScale(.5f);
                lastNode = nodeOutlet;

                var modelOutlet = nodeOutlet.CreateComponent<StaticModel>();
                modelOutlet.Model = ResourceCache.GetModel("Data\\LowPressurePipeLayer.mdl");

                var cubeMaterial = Material.FromColor(Color.Gray);
                modelOutlet.SetMaterial(cubeMaterial);


                //Push to UWP
                var textNode = outletBase.CreateChild("Text");
                var text = textNode.CreateComponent<Text3D>();
                BulbAddedDto bulb = new BulbAddedDto { scale_factor = 0, obj_name = "drum", Text = "", ID = outletBase.Name, Position = new Vector3Dto(cursor.CursorNode.WorldPosition.X, cursor.CursorNode.WorldPosition.Y, cursor.CursorNode.WorldPosition.Z), Direction = new Vector3Dto(LeftCamera.Node.WorldDirection.X, LeftCamera.Node.WorldDirection.Y, LeftCamera.Node.WorldDirection.Z) };

                ExistingBulbs.TryAdd(bulb.ID, bulb);
                //SAVE
                outletNode = outletBase;
            }
        }


        protected override void OnUpdate(float timeStep)
        {
            base.OnUpdate(timeStep);
            DirectionalLight.Node.SetDirection(LeftCamera.Node.Direction);

            HandleMovementByKeyPress(Input, 1f);

        }

        private void HandleMovementByKeyPress(Input input, float duration)
        {
            Urho.Actions.FiniteTimeAction action = null;

            if (input.GetKeyPress(Urho.Key.W))
            {
                action = new MoveBy(duration, new Vector3(0, 0, 1));
            }

            if (input.GetKeyPress(Urho.Key.S))
            {
                action = new MoveBy(duration, new Vector3(0, 0, -1));
            }
            if (input.GetKeyPress(Urho.Key.A))
            {
                action = new MoveBy(duration, new Vector3(-1, 0, 0));
            }
            if (input.GetKeyPress(Urho.Key.D))
            {
                action = new MoveBy(duration, new Vector3(1, 0, 0));
            }
            if (input.GetKeyPress(Urho.Key.Q))
            {
                action = new MoveBy(duration, new Vector3(0, 1, 0));
            }
            if (input.GetKeyPress(Urho.Key.E))
            {
                action = new MoveBy(duration, new Vector3(0, -1, 0));
            }


            //if (input.GetKeyPress(Urho.Key.K))
            //{
            //    boxNode.RunActionsAsync(new RotateBy(duration, 0, 10, 0));
            //}
            //if (input.GetKeyPress(Urho.Key.L))
            //{
            //    boxNode.RunActionsAsync(new RotateBy(duration, 0, -10, 0));
            //}
            //if (input.GetKeyPress(Urho.Key.J))
            //{
            //    boxNode.RunActionsAsync(new RotateBy(duration, 10, 0, 0));
            //}
            //if (input.GetKeyPress(Urho.Key.I))
            //{
            //    boxNode.RunActionsAsync(new RotateBy(duration, 10, 0, 0));
            //}
            //if (input.GetKeyPress(Urho.Key.K))
            //{
            //    boxNode.RunActionsAsync(new RotateBy(duration, -10, 0, 0));
            //}
            //if (input.GetKeyPress(Urho.Key.O))
            //{
            //    boxNode.RunActionsAsync(new RotateBy(duration, 0, 0, 10));
            //}
            //if (input.GetKeyPress(Urho.Key.U))
            //{
            //    boxNode.RunActionsAsync(new RotateBy(duration, 0, 0, -10));
            //}
            //if (input.GetKeyPress(Urho.Key.T))
            //{
            //    if (box.Color == Urho.Color.Transparent)
            //    {
            //        box.Color = Urho.Color.Blue;
            //    }
            //    else
            //    {
            //        box.Color = Urho.Color.Transparent;
            //    }
            //}



            if (action != null)
            {
                //can be awaited
                lastNode.RunActionsAsync(action);
            }
        }
        public async override void OnGestureTapped()
        {
            if (cursor == null)
                return;


            //var noteNode = Raycast();
            //if (noteNode != null)
            //{
            //    if (ExistingBulbs.ContainsKey(noteNode.Name))
            //    {
            //        foreach (var childNode in noteNode.Children)
            //        {
            //            if (childNode.Name == "Text")
            //            {
            //                childNode.Enabled = !childNode.Enabled;
            //                return;
            //            }
            //        }
            //    }
            //}


            //var speechRecognizer = new Windows.Media.SpeechRecognition.SpeechRecognizer();
            //// Compile the dictation grammar by default.
            //await speechRecognizer.CompileConstraintsAsync();
            //string speechText = "";
            //// Start recognition.
            //var pos = cursor.CursorNode.WorldPosition;
            //var dir = LeftCamera.Node.WorldDirection;
            //try
            //{
            //    Windows.Media.SpeechRecognition.SpeechRecognitionResult speechRecognitionResult = await speechRecognizer.RecognizeAsync();
            //    speechText = speechRecognitionResult.Text;
            //}
            //catch
            //{
            //    return;
            //}
            //var child = Scene.CreateChild(Guid.NewGuid().ToString());
            //child.Scale = new Vector3(1, 1f, 1) / 10;

            //child.Position = pos;
            //child.LookAt(dir, Vector3.UnitY, TransformSpace.Local);
            //child.Position = new Vector3(pos.X, pos.Y, pos.Z - .05f);
            //child.Rotate(new Quaternion(315f, 270f, 0f), TransformSpace.Local);

            //var model = child.CreateComponent<StaticModel>();
            //model.Model = ResourceCache.GetModel("Data\\thumbtack.mdl");
            //var textNode = child.CreateChild("Text");
            //textNode.Rotate(new Quaternion(-315f, -270f, 0f), TransformSpace.Local);
            //var text = textNode.CreateComponent<Text3D>();
            //text.Text = speechText;
            //text.HorizontalAlignment = HorizontalAlignment.Center;
            //text.VerticalAlignment = VerticalAlignment.Center;
            //text.TextAlignment = HorizontalAlignment.Center;
            //text.SetFont(CoreAssets.Fonts.AnonymousPro, 20);
            //text.SetColor(Color.Green);
            ////text.Opacity = 0f;

            //BulbAddedDto bulb = new BulbAddedDto { ID = child.Name, Position = new Vector3Dto(pos.X, pos.Y, pos.Z), Text = speechText, Direction = new Vector3Dto(LeftCamera.Node.WorldDirection.X, LeftCamera.Node.WorldDirection.Y, LeftCamera.Node.WorldDirection.Z) };


            //ExistingBulbs.TryAdd(bulb.ID, bulb);
        }

        public override unsafe void OnSurfaceAddedOrUpdated(SpatialMeshInfo surface, Model generatedModel)
        {
            bool isNew = false;
            StaticModel staticModel = null;
            _surfaceID = surface.SurfaceId;
            Node node = environmentNode.GetChild(surface.SurfaceId, false);
            if (node != null)
            {
                isNew = false;
                staticModel = node.GetComponent<StaticModel>();
            }
            else
            {
                isNew = true;
                node = environmentNode.CreateChild(surface.SurfaceId);
                staticModel = node.CreateComponent<StaticModel>();
            }

            node.Position = surface.BoundsCenter;
            node.Rotation = surface.BoundsRotation;
            staticModel.Model = generatedModel;


            if (isNew)
            {
                staticModel.SetMaterial(material);
            }

            var surfaceDto = new SurfaceDto
            {
                Id = surface.SurfaceId,
                IndexData = surface.IndexData,
                BoundsCenter = new Vector3Dto(surface.BoundsCenter.X, surface.BoundsCenter.Y, surface.BoundsCenter.Z),
                BoundsOrientation = new Vector4Dto(surface.BoundsRotation.X,
                    surface.BoundsRotation.Y, surface.BoundsRotation.Z, surface.BoundsRotation.W),
                BoundsExtents = new Vector3Dto(surface.Extents.X, surface.Extents.Y, surface.Extents.Z)
            };

            var vertexData = surface.VertexData;
            surfaceDto.VertexData = new SpatialVertexDto[vertexData.Length];
            for (int i = 0; i < vertexData.Length; i++)
            {
                SpatialVertex vertexItem = vertexData[i];
                surfaceDto.VertexData[i] = *(SpatialVertexDto*)(void*)&vertexItem;
            }

        }

        BaseDto GetCurrentPositionDto()
        {
            var position = LeftCamera.Node.Position;
            var direction = LeftCamera.Node.Direction;
            return new CurrentPositionDto
            {
                SessionID = SessionID.ToString(),
                Position = new Vector3Dto(position.X, position.Y, position.Z),
                Direction = new Vector3Dto(direction.X, direction.Y, direction.Z)
            };
        }


        Node Raycast()
        {
            Ray cameraRay = LeftCamera.GetScreenRay(0.5f, 0.5f);
            var result = Scene.GetComponent<Octree>().RaycastSingle(cameraRay, RayQueryLevel.Triangle, 100, DrawableFlags.Geometry, 0x70000000);
            if (result != null)
            {
                return result.Value.Node;
            }
            return null;
        }
#if VIDEO_RECORDING
		TaskCompletionSource<string> fakeQrCodeResultTaskSource = new TaskCompletionSource<string>();
		public override void OnGestureDoubleTapped()
		{
			// Unfortunately, it's not allowed to record a video ("Hey Cortana, start recording")
			// and grab frames (in order to read a QR) at the same time - it will crash.
			// so I use a fake QR code result for the demo purposes
			// it is emulated by a double tap gesture
			Task.Run(() => fakeQrCodeResultTaskSource.TrySetResult("192.168.1.6:5206"));
		}
#endif
    }
 //   public class MutantApp : HoloApplication
	//{
	//	Node mutantNode;
	//	Vector3 monsterPositionBeforeManipulations;
	//	AnimationController animation;

	//	const string IdleAni = "Mutant_Idle1.ani";
	//	const string KillAni = "Mutant_Death.ani";
	//	const string HipHopAni = "Mutant_HipHop1.ani";
	//	const string JumpAni = "Mutant_Jump.ani";
	//	const string JumpAttack = "Mutant_JumpAttack.ani";
	//	const string KickAni = "Mutant_Kick.ani";
	//	const string PunchAni = "Mutant_Punch.ani";
	//	const string RunAni = "Mutant_Run.ani";
	//	const string SwipeAni = "Mutant_Swipe.ani";
	//	const string WalkAni = "Mutant_Walk.ani";

	//	public MutantApp(ApplicationOptions opts) : base(opts) { }

	//	protected override void Start()
	//	{
	//		base.Start();

	//		EnableGestureManipulation = true;

	//		Zone.AmbientColor = new Color(0.8f, 0.8f, 0.8f);
	//		DirectionalLight.Brightness = 1;

	//		mutantNode = Scene.CreateChild();
	//		mutantNode.Position = new Vector3(0, 0, 2f);
	//		mutantNode.SetScale(0.2f);
	//		var mutant = mutantNode.CreateComponent<AnimatedModel>();

	//		mutant.Model = ResourceCache.GetModel("Models/Mutant.mdl");
	//		mutant.SetMaterial(ResourceCache.GetMaterial("Materials/mutant_M.xml"));
	//		animation = mutantNode.CreateComponent<AnimationController>();
	//		PlayAnimation(IdleAni);

	//		RegisterCortanaCommands(new Dictionary<string, Action>
	//			{
	//				//play animations using Cortana
	//				{"idle", () => PlayAnimation(IdleAni)},
	//				{"die", () => PlayAnimation(KillAni)},
	//				{"dance", () => PlayAnimation(HipHopAni)},
	//				{"jump", () => PlayAnimation(JumpAni)},
	//				{"jump attack", () => PlayAnimation(JumpAttack)},
	//				{"kick", () => PlayAnimation(KickAni)},
	//				{"punch", () => PlayAnimation(PunchAni)},
	//				{"run", () => PlayAnimation(RunAni)},
	//				{"swipe", () => PlayAnimation(SwipeAni)},
	//				{"walk", () => PlayAnimation(WalkAni)},

	//				{"bigger", () => mutantNode.ScaleNode(1.2f)},
	//				{"smaller", () => mutantNode.ScaleNode(0.8f)},
	//				{"increase the brightness", () => IncreaseBrightness(1.2f)},
	//				{"decrease the brightness", () => IncreaseBrightness(0.8f)},
	//				{"look at me", LookAtMe },
	//				{"turn around", () => mutantNode.RunActions(new RotateBy(1f, 0, 180, 0))},
	//				{"help", Help },                    
	//				{"back", ShowXaml }                    
	//			});
	//	}

	//    private async void ShowXaml()
	//    {
 //           await ApplicationViewSwitcher.SwitchAsync(App.ViewXaml.Id);
 //       }

	//    void LookAtMe()
	//	{
	//		mutantNode.Rotation = new Quaternion(0, -LeftCamera.Node.Rotation.ToEulerAngles().Y, 0);
	//		//or mutantNode.LookAt(...);
	//	}

	//	async void Help()
	//	{
	//		await TextToSpeech("Available commands are:");
	//		foreach (var cortanaCommand in CortanaCommands.Keys)
	//			await TextToSpeech(cortanaCommand);
	//	}

	//	void IncreaseBrightness(float byValue)
	//	{
	//		//by default, HoloScene has two kinds of lights:
	//		DirectionalLight.Brightness *= byValue;
	//	}

	//	void PlayAnimation(string file, bool looped = true)
	//	{
	//		mutantNode.RemoveAllActions();

	//		if (file == WalkAni)
	//			mutantNode.RunActions(new RepeatForever(new MoveBy(1f, mutantNode.Rotation * new Vector3(0, 0, -mutantNode.Scale.X))));
	//		else if (file == RunAni)
	//			mutantNode.RunActions(new RepeatForever(new MoveBy(1f, mutantNode.Rotation * new Vector3(0, 0, -mutantNode.Scale.X * 2))));

	//		animation.StopAll(0.2f);
	//		animation.Play("Animations/" + file, 0, looped, 0.2f);
	//	}


	//	protected override void OnUpdate(float timeStep)
	//	{
	//		base.OnUpdate(timeStep);

	//		//for optical stabilization:
	//		//TODO: PostUpdate?
	//		FocusWorldPoint = mutantNode.WorldPosition;
	//	}

	//	// Handle spatial input gestures:

	//	public override void OnGestureManipulationStarted()
	//	{
	//		monsterPositionBeforeManipulations = mutantNode.Position;
	//	}

	//	public override void OnGestureManipulationUpdated(Vector3 relativeHandPosition)
	//	{
	//		mutantNode.Position = relativeHandPosition * 2 + monsterPositionBeforeManipulations;
	//	}
	//}
}