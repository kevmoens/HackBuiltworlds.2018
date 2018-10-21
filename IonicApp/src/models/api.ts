export class Transform {
    idTransform: number
    name: string
    description: string
    
    constructor(values: Object = {}) {
        Object.assign(this, values);
    }
}

export class Criteria {
    id: number
    name: String
    description: String
    rating: number
    created_at: Date
    updated_at: Date
}

