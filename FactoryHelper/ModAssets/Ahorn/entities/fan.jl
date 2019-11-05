module FactoryHelperFan

using ..Ahorn, Maple

@mapdef Entity "FactoryHelper/FanHorizontal" FanHorizontal(x::Integer, y::Integer, width::Integer=24, startActive::Bool=false, activationId::String="")
@mapdef Entity "FactoryHelper/FanVertical" FanVertical(x::Integer, y::Integer, height::Integer=24, startActive::Bool=false, activationId::String="")

fanUnion = Union{FanHorizontal, FanVertical}

const placements = Ahorn.PlacementDict(
    "Fan (Horizontal, Inactive) (FactoryHelper)" => Ahorn.EntityPlacement(
        FanHorizontal,
        "rectangle",
    ),
    "Fan (Vertical, Inactive) (FactoryHelper)" => Ahorn.EntityPlacement(
        FanVertical,
        "rectangle",
    ),
    "Fan (Horizontal, Active) (FactoryHelper)" => Ahorn.EntityPlacement(
        FanHorizontal,
        "rectangle",
        Dict{String, Any}(
			"startActive" => true,
        )
    ),
    "Fan (Vertical, Active) (FactoryHelper)" => Ahorn.EntityPlacement(
        FanVertical,
        "rectangle",
        Dict{String, Any}(
			"startActive" => true,
        )
    ),
)
    
bodySprite = "objects/FactoryHelper/fan/body0"
fanSprite = "objects/FactoryHelper/fan/fan0"

directions = Dict{String, String}(
    "FactoryHelper/FanHorizontal" => "Horizontal",
    "FactoryHelper/FanVertical" => "Vertical",
)

directionRotation = Dict{String,Real}(
    "Horizontal" => 0,
    "Vertical" => -pi/2,
)

minimumSizes = Dict{String,Tuple{Integer,Integer}}(
    "Horizontal" => (24,16),
    "Vertical" => (16,24),
)

rotationDisplacements = Dict{String,Tuple{Integer,Integer}}(
    "Horizontal" => (0,0),
    "Vertical" => (4,20),
)

resizeDirections = Dict{String,Tuple{Bool,Bool}}(
    "Horizontal" => (true,false),
    "Vertical" => (false,true),
)

function Ahorn.resizable(entity::fanUnion)
	direction = get(directions, entity.name, "Horizontal")

	return resizeDirections[direction]
end

function Ahorn.minimumSize(entity::fanUnion)
	direction = get(directions, entity.name, "Horizontal")

	return minimumSizes[direction]
end

function Ahorn.renderAbs(ctx::Ahorn.Cairo.CairoContext, entity::fanUnion, room::Maple.Room)
    x, y = Ahorn.position(entity)
	direction = get(directions, entity.name, "Horizontal")
    width = get(entity.data, "width", 0)
    height = get(entity.data, "height", 0)
    dx, dy = rotationDisplacements[direction]
    
    Ahorn.drawRectangle(ctx, x, y, width, height, (0.5, 0.4, 0.3, 0.4), (0.5, 0.4, 0.3, 1.0))
    Ahorn.drawSprite(ctx, fanSprite, x + width/2 + dx, y + height/2 + dy, rot=directionRotation[direction])
    Ahorn.drawSprite(ctx, bodySprite, x + width/2 + dx, y + height/2 + dy, rot=directionRotation[direction])
end

function Ahorn.selection(entity::fanUnion)
    x, y = Ahorn.position(entity)
    width = get(entity.data, "width", 0)
    height = get(entity.data, "height", 0)

    return Ahorn.Rectangle[Ahorn.Rectangle(x,y,width,height)]
end

end