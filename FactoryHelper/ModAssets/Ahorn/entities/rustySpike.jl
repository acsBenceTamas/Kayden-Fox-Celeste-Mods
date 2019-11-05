module FactoryHelperRustySpike

using ..Ahorn, Maple

@mapdef Entity "FactoryHelper/RustySpikeUp" RustySpikeUp(x::Integer, y::Integer, width::Integer=8)
@mapdef Entity "FactoryHelper/RustySpikeDown" RustySpikeDown(x::Integer, y::Integer, width::Integer=8)
@mapdef Entity "FactoryHelper/RustySpikeLeft" RustySpikeLeft(x::Integer, y::Integer, height::Integer=8)
@mapdef Entity "FactoryHelper/RustySpikeRight" RustySpikeRight(x::Integer, y::Integer, height::Integer=8)

rustySpikes = Union{RustySpikeUp, RustySpikeDown, RustySpikeLeft, RustySpikeRight}

directions = Dict{String, String}(
    "FactoryHelper/RustySpikeUp" => "up",
    "FactoryHelper/RustySpikeDown" => "down",
    "FactoryHelper/RustySpikeLeft" => "left",
    "FactoryHelper/RustySpikeRight" => "right",
)

const placements = Ahorn.PlacementDict(
    "Spikes (Up, Rusty) (FactoryHelper)" => Ahorn.EntityPlacement(
        RustySpikeUp,
        "rectangle",
    ),
    "Spikes (Down, Rusty) (FactoryHelper)" => Ahorn.EntityPlacement(
        RustySpikeDown,
        "rectangle",
    ),
    "Spikes (Left, Rusty) (FactoryHelper)" => Ahorn.EntityPlacement(
        RustySpikeLeft,
        "rectangle",
    ),
    "Spikes (Right, Rusty) (FactoryHelper)" => Ahorn.EntityPlacement(
        RustySpikeRight,
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

function Ahorn.renderSelectedAbs(ctx::Ahorn.Cairo.CairoContext, entity::rustySpikes)
    direction = get(directions, entity.name, "up")
    theta = rotations[direction] - pi / 2

    width = Int(get(entity.data, "width", 0))
    height = Int(get(entity.data, "height", 0))

    x, y = Ahorn.position(entity)
    cx, cy = x + floor(Int, width / 2) - 8 * (direction == "left"), y + floor(Int, height / 2) - 8 * (direction == "up")

    Ahorn.drawArrow(ctx, cx, cy, cx + cos(theta) * 24, cy + sin(theta) * 24, Ahorn.colors.selection_selected_fc, headLength=6)
end

function Ahorn.selection(entity::rustySpikes)
	x, y = Ahorn.position(entity)

	width = Int(get(entity.data, "width", 8))
	height = Int(get(entity.data, "height", 8))

	direction = get(directions, entity.name, "up")

	ox, oy = offsets[direction]

	return Ahorn.Rectangle(x + ox - 4, y + oy - 4, width, height)
end

function Ahorn.minimumSize(entity::rustySpikes)
	return (8, 8)
end

function Ahorn.resizable(entity::rustySpikes)
	direction = get(directions, entity.name, "up")

	return resizeDirections[direction]
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::rustySpikes)
        direction = get(directions, entity.name, "up")      
		width = get(entity.data, "width", 8)
		height = get(entity.data, "height", 8)

		for ox in 0:8:width - 8, oy in 0:8:height - 8
			drawX, drawY = (ox, oy) .+ offsets[direction]
			Ahorn.drawSprite(ctx, "objects/FactoryHelper/rustySpike/rusty_$(direction)00", drawX, drawY)
		end
end

end
