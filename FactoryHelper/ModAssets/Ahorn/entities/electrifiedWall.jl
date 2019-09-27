module FactoryHelperElectrifiedWall

using ..Ahorn, Maple

@mapdef Entity "FactoryHelper/ElectrifiedWallUp" ElectrifiedWallUp(x::Integer, y::Integer, width::Integer=8, activationId::String="", startActive::Bool=true)
@mapdef Entity "FactoryHelper/ElectrifiedWallDown" ElectrifiedWallDown(x::Integer, y::Integer, width::Integer=8, activationId::String="", startActive::Bool=true)
@mapdef Entity "FactoryHelper/ElectrifiedWallLeft" ElectrifiedWallLeft(x::Integer, y::Integer, height::Integer=8, activationId::String="", startActive::Bool=true)
@mapdef Entity "FactoryHelper/ElectrifiedWallRight" ElectrifiedWallRight(x::Integer, y::Integer, height::Integer=8, activationId::String="", startActive::Bool=true)

ElectrifiedWalls = Union{ElectrifiedWallUp, ElectrifiedWallDown, ElectrifiedWallLeft, ElectrifiedWallRight}

directions = Dict{String, String}(
    "FactoryHelper/ElectrifiedWallUp" => "up",
    "FactoryHelper/ElectrifiedWallDown" => "down",
    "FactoryHelper/ElectrifiedWallLeft" => "left",
    "FactoryHelper/ElectrifiedWallRight" => "right",
)

const placements = Ahorn.PlacementDict(
    "Spike (Up, Electrified) (FactoryHelper)" => Ahorn.EntityPlacement(
        ElectrifiedWallUp,
        "rectangle",
    ),
    "Spike (Down, Electrified) (FactoryHelper)" => Ahorn.EntityPlacement(
        ElectrifiedWallDown,
        "rectangle",
    ),
    "Spike (Left, Electrified) (FactoryHelper)" => Ahorn.EntityPlacement(
        ElectrifiedWallLeft,
        "rectangle",
    ),
    "Spike (Right, Electrified) (FactoryHelper)" => Ahorn.EntityPlacement(
        ElectrifiedWallRight,
        "rectangle",
    ),
)

offsets = Dict{String, Tuple{Integer, Integer}}(
    "up" => (4, -4),
    "down" => (4, 4),
    "left" => (-4, 4),
    "right" => (4, 4),
)

rotations = Dict{String, Number}(
    "up" => 0,
    "right" => pi / 2,
    "down" => pi,
    "left" => pi * 3 / 2
)

rotationOffsets = Dict{String, Tuple{Number, Number}}(
    "up" => (0.5, 0.25),
    "right" => (1, 0.675),
    "down" => (1.5, 1.125),
    "left" => (0, 1.675)
)

resizeDirections = Dict{String, Tuple{Bool, Bool}}(
    "up" => (true, false),
    "down" => (true, false),
    "left" => (false, true),
    "right" => (false, true),
)

drawOffsets = Dict{String, Tuple{Integer, Integer}}(
    "up" => (1, 0),
    "down" => (1, 0),
    "left" => (0, 1),
    "right" => (0, 1),
)

powerFieldOffsets = Dict{String, Tuple{Integer, Integer}}(
    "up" => (0, -8),
    "down" => (0, 0),
    "left" => (-8, 0),
    "right" => (0, 0),
)

sizeString = Dict{String, String}(
    "up" => "width",
    "down" => "width",
    "left" => "height",
    "right" => "height",
)

function Ahorn.renderSelectedAbs(ctx::Ahorn.Cairo.CairoContext, entity::ElectrifiedWalls)
    direction = get(directions, entity.name, "up")
    theta = rotations[direction] - pi / 2

    width = Int(get(entity.data, "width", 0))
    height = Int(get(entity.data, "height", 0))

    x, y = Ahorn.position(entity)
    cx, cy = x + floor(Int, width / 2) - 8 * (direction == "left"), y + floor(Int, height / 2) - 8 * (direction == "up")

    Ahorn.drawArrow(ctx, cx, cy, cx + cos(theta) * 24, cy + sin(theta) * 24, Ahorn.colors.selection_selected_fc, headLength=6)
end

function Ahorn.selection(entity::ElectrifiedWalls)
	x, y = Ahorn.position(entity)

	width = Int(get(entity.data, "width", 8))
	height = Int(get(entity.data, "height", 8))

	direction = get(directions, entity.name, "up")

	ox, oy = offsets[direction]

	return Ahorn.Rectangle(x + ox - 4, y + oy - 4, width, height)
end

function Ahorn.minimumSize(entity::ElectrifiedWalls)
	return (8, 8)
end

function Ahorn.resizable(entity::ElectrifiedWalls)
	direction = get(directions, entity.name, "up")

	return resizeDirections[direction]
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::ElectrifiedWalls)
        direction = get(directions, entity.name, "up")
		size = get(entity.data, sizeString[direction], 8)
		width = Int(get(entity.data, "width", 8))
		height = Int(get(entity.data, "height", 8))

		drawX, drawY = offsets[direction]
		Ahorn.drawSprite(ctx, "objects/FactoryHelper/electrifiedWall/knob_$(direction)00", drawX, drawY)
		drawX, drawY = offsets[direction] .+ drawOffsets[direction] .* (size-8)
		Ahorn.drawSprite(ctx, "objects/FactoryHelper/electrifiedWall/knob_$(direction)00", drawX, drawY)
		
		pfX, pfY = powerFieldOffsets[direction]
		
        Ahorn.drawRectangle(ctx, pfX, pfY, width, height, (0.0, 0.6, 0.6, 0.2), (0.0, 0.6, 0.6, 0.5))
		
end

end
