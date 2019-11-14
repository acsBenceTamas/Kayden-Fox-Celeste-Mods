module TrollLandTrollSpring

using ..Ahorn, Maple

@mapdef Entity "TrollLand/TrollSpring" TrollSpring(x::Integer, y::Integer, orientation::String="FloorLeft")

orientations = ["FloorLeft", "FloorRight", "WallLeftUp", "WallLeftDown", "WallRightUp", "WallRightDown"]

sprite = "objects/TrollLand/trollSpring/00"

const placements = Ahorn.PlacementDict(
    "Troll Spring ($(orientation)) (Troll Land)" => Ahorn.EntityPlacement(
        TrollSpring,
		"point",
		Dict{String, Any}("orientation" => orientation)
    ) for orientation in orientations
)

Ahorn.editingOptions(entity::TrollSpring) = Dict{String, Any}(
    "orientation" => orientations
)

function getSelection(entity::TrollSpring)
    x, y = Ahorn.position(entity)
	orientation = get(entity, "orientation", "FloorLeft")
	
	rectangle = Ahorn.Rectangle
	
	if (orientation == "FloorLeft" || orientation == "FloorRight")
		rectangle = Ahorn.Rectangle(x - 6, y - 3, 12, 5)
	elseif (orientation == "WallLeftUp" || orientation == "WallLeftDown")
		rectangle = Ahorn.Rectangle(x - 2, y - 6, 5, 12)
	elseif (orientation == "WallRightUp" || orientation == "WallRightDown")
		rectangle = Ahorn.Rectangle(x - 3, y - 6, 5, 12)
	end
	
	return rectangle
end

function drawSprite(ctx::Ahorn.Cairo.CairoContext, entity::TrollSpring, room::Maple.Room)
	orientation = get(entity, "orientation", "FloorLeft")
	
	if (orientation == "FloorLeft" || orientation == "FloorRight")
		Ahorn.drawSprite(ctx, sprite, 0, -8)
	elseif (orientation == "WallLeftUp" || orientation == "WallLeftDown")
		Ahorn.drawSprite(ctx, sprite, 24, 0, rot=pi / 2)
	elseif (orientation == "WallRightUp" || orientation == "WallRightDown")
		Ahorn.drawSprite(ctx, sprite, -8, 16, rot=-pi / 2)
	end
end

function Ahorn.selection(entity::TrollSpring)
    return getSelection(entity)
end

Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::TrollSpring, room::Maple.Room) = drawSprite(ctx, entity, room)

end