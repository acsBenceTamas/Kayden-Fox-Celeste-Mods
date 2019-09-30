module FactoryHelperConveyor

using ..Ahorn, Maple

@mapdef Entity "FactoryHelper/Conveyor" Conveyor(x::Integer, y::Integer, width::Integer=32, activationId::String="", startLeft::Bool=true)

const placements = Ahorn.PlacementDict(
    "Conveyor (Left) (FactoryHelper)" => Ahorn.EntityPlacement(
        Conveyor,
        "rectangle",
        Dict{String, Any}(
            "startLeft" => true,
        )
    ),
    "Conveyor (Right) (FactoryHelper)" => Ahorn.EntityPlacement(
        Conveyor,
        "rectangle",
        Dict{String, Any}(
            "startLeft" => false,
        )
    ),
)

function Ahorn.minimumSize(entity::Conveyor)
	return (32, 16)
end

function Ahorn.resizable(entity::Conveyor)
	return (true, false)
end

function Ahorn.renderSelectedAbs(ctx::Ahorn.Cairo.CairoContext, entity::Conveyor)
    width = Int(get(entity.data, "width", 32))

    x, y = Ahorn.position(entity)
end

function Ahorn.selection(entity::Conveyor)
	x, y = Ahorn.position(entity)

	width = Int(get(entity.data, "width", 8))
	height = Int(get(entity.data, "height", 8))

	return Ahorn.Rectangle(x, y, width, height)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::Conveyor)
		width = get(entity.data, "width", 8)

		Ahorn.drawSprite(ctx, "objects/FactoryHelper/conveyor/belt_edge0", 0, 0, jx=0, jy=0)
		Ahorn.drawSprite(ctx, "objects/FactoryHelper/conveyor/belt_edge4", width, 0, sx=-1, jx=0, jy=0)

		Ahorn.drawSprite(ctx, "objects/FactoryHelper/conveyor/gear0", 0, 0, jx=0, jy=0)
		Ahorn.drawSprite(ctx, "objects/FactoryHelper/conveyor/gear0", width, 0, sx=-1, jx=0, jy=0)

		for ox in 0:8:width - 40
			Ahorn.drawSprite(ctx, "objects/FactoryHelper/conveyor/belt_mid0", ox + 16, 0, jx=0, jy=0)
		end
		
		if get(entity.data, "startLeft", true)
			Ahorn.drawArrow(ctx, width - 8, 8, 8, 8, (0.0, 0.0, 0.7, 1.0) , headLength=4)
		else
			Ahorn.drawArrow(ctx, 8, 8, width - 8, 8, (0.0, 0.0, 0.7, 1.0) , headLength=4)
		end
end

end