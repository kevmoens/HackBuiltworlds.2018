using Shared;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Urho;
using Urho.Actions;
using Urho.Gui;
using Urho.HoloLens;
using Urho.Shapes;

namespace SmartHome.HoloLens
{
	public class ScannerApp : HoloApplication
	{
        Node menuNode;
        Node outletNode;
        Node heaterNode;
        Node heaterNodeSave;
        Node environmentNode;
        bool IsXRay = true;
		SpatialCursor cursor;
		Material material;
        Guid SessionID;
        bool IsPrimary; //Is Primary Session 
        string _surfaceID;
        bool _Debug;
        System.Threading.Timer timer;
        System.Collections.Concurrent.ConcurrentDictionary<string, BulbAddedDto> ExistingBulbs;

        public ScannerApp(ApplicationOptions opts) : base(opts) {
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
            
            Action placeOutlet = async delegate ()
            {
                var outletBase = Scene.CreateChild("OUTLET");
                outletBase.Scale = new Vector3(1, 1f, 1) / 10;
                outletBase.Position = cursor.CursorNode.WorldPosition;
                outletBase.SetDirection(cursor.CursorNode.WorldDirection);


                var nodeOutlet = outletBase.CreateChild();
            //nodeOutlet.Rotate(new Quaternion(0, 90, 90, 90), TransformSpace.Local);


            nodeOutlet.Rotation = new Quaternion( 0, 270, -90); // cursor.CursorNode.Rotation.ToEulerAngles().Y, 0);
                //nodeOutlet.RotateAround(new Vector3(0, 0, 0), new Quaternion(0, 270, 90), TransformSpace.Local); //KMM


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
                if (outletNode != null)
                {
                    try
                    {
                        outletNode.Remove();
                        outletNode.Dispose();
                        outletNode = null;
                    }
                    catch { }
                }
            };
            await RegisterCortanaCommands(new Dictionary<string, Action>() {
                { "place outlet", PlaceOutletModel}
                , {"remove outlet", RemoveOutlet}
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
      
                var outletBase = Scene.CreateChild("OUTLET");
                outletBase.Scale = new Vector3(1, 1f, 1) / 10;
                outletBase.Position = result.Value.Position;

                var nodeOutlet = outletBase.CreateChild();

                if (result.Value.Normal != Vector3.Zero)
                {
                    if (result.Value.Normal.X != 0)
                    {
                        nodeOutlet.Rotation = Quaternion.FromRotationTo(Vector3.Back, result.Value.Normal);
                    } else
                    {
                        nodeOutlet.Rotation = Quaternion.FromRotationTo(Vector3.Right, result.Value.Normal);
                    }
                }

                nodeOutlet.Position -= (result.Value.Normal * 0.25f);

                nodeOutlet.SetScale(.5f);


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

                var outletBase = Scene.CreateChild("OUTLET");
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

                nodeOutlet.Position += (result.Value.Normal * 0.25f);

                nodeOutlet.SetScale(.5f);


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



        private static void WaterHeaterData(Node heaterBase, Quaternion rotation, Vector3 position, string Text)
        {
            var nodeHeaterDesc = heaterBase.CreateChild();
            nodeHeaterDesc.Rotation = rotation;
            nodeHeaterDesc.Position = position;
            nodeHeaterDesc.SetScale(20f);
            var text = nodeHeaterDesc.CreateComponent<Text3D>();
            text.Text = Text;
            text.HorizontalAlignment = HorizontalAlignment.Center;
            text.VerticalAlignment = VerticalAlignment.Center;
            text.TextAlignment = HorizontalAlignment.Center;
            text.SetFont(CoreAssets.Fonts.AnonymousPro, 20);
            text.SetColor(Color.Green);
        }


		protected override void OnUpdate(float timeStep)
		{
			base.OnUpdate(timeStep);
			DirectionalLight.Node.SetDirection(LeftCamera.Node.Direction);
		}

		public async override void OnGestureTapped()
		{
			if (cursor == null)
				return;


            var noteNode = Raycast();
            if (noteNode != null)
            {
                if (ExistingBulbs.ContainsKey(noteNode.Name))
                {
                    foreach (var childNode in noteNode.Children)
                    {
                        if (childNode.Name == "Text")
                        {
                            childNode.Enabled = !childNode.Enabled;
                            return;
                        }
                    }
                }
            }
      

            var speechRecognizer = new Windows.Media.SpeechRecognition.SpeechRecognizer();
            // Compile the dictation grammar by default.
            await speechRecognizer.CompileConstraintsAsync();
            string speechText = "";
            // Start recognition.
            var pos = cursor.CursorNode.WorldPosition;
            var dir = LeftCamera.Node.WorldDirection;
            try
            {
                Windows.Media.SpeechRecognition.SpeechRecognitionResult speechRecognitionResult = await speechRecognizer.RecognizeAsync();
                speechText = speechRecognitionResult.Text;
            }
            catch {
                return;
            }
            var child = Scene.CreateChild(Guid.NewGuid().ToString());
			child.Scale = new Vector3(1, 1f, 1) / 10;
           
			child.Position = pos;
            child.LookAt(dir , Vector3.UnitY, TransformSpace.Local);
            child.Position = new Vector3(pos.X , pos.Y , pos.Z - .05f );
            child.Rotate(new Quaternion(315f, 270f, 0f), TransformSpace.Local);

            var model = child.CreateComponent<StaticModel>();
            model.Model = ResourceCache.GetModel("Data\\thumbtack.mdl");
            var textNode = child.CreateChild("Text");
            textNode.Rotate(new Quaternion(-315f, -270f, 0f), TransformSpace.Local);
            var text = textNode.CreateComponent<Text3D>();
            text.Text = speechText;
            text.HorizontalAlignment = HorizontalAlignment.Center;
            text.VerticalAlignment = VerticalAlignment.Center;
            text.TextAlignment = HorizontalAlignment.Center;
            text.SetFont(CoreAssets.Fonts.AnonymousPro, 20);
            text.SetColor(Color.Green);
            //text.Opacity = 0f;

            BulbAddedDto bulb = new BulbAddedDto { ID = child.Name, Position = new Vector3Dto(pos.X, pos.Y, pos.Z), Text = speechText, Direction = new Vector3Dto(LeftCamera.Node.WorldDirection.X, LeftCamera.Node.WorldDirection.Y, LeftCamera.Node.WorldDirection.Z) };


            ExistingBulbs.TryAdd(bulb.ID, bulb);
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
}