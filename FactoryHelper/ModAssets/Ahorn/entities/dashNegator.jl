module FactoryHelperDashNegator

using ..Ahorn, Maple

@mapdef Entity "FactoryHelper/DashNegator" DashNegator(x::Integer, y::Integer, width::Integer=16, height::Integer=32, activationId::String="", startActive::Bool=true)

const placements = Ahorn.PlacementDict(
    "DashNegator (FactoryHelper)" => Ahorn.EntityPlacement(
        DashNegator,
        "rectangle",
    ),
)

function Ahorn.minimumSize(entity::DashNegator)
	return (16, 32)
end

function Ahorn.resizable(entity::DashNegator)
	return (true, true)
end

function Ahorn.renderSelectedAbs(ctx::Ahorn.Cairo.CairoContext, entity::DashNegator)
end

function Ahorn.selection(entity::DashNegator)
	x, y = Ahorn.position(entity)

	width = Int(get(entity.data, "width", 16))
	height = Int(get(entity.data, "height", 32))

	return Ahorn.Rectangle(x, y, width, height)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::DashNegator)
		width = floor((get(entity.data, "width", 16)) / 16) * 16
		height = get(entity.data, "height", 32)
		sprite = String
		
		if(get(entity.data, "startActive", true))
			sprite = "danger/FactoryHelper/dashNegator/turret05"
		else
			sprite = "danger/FactoryHelper/dashNegator/turret00"
		end
		
		Ahorn.drawRectangle(ctx, 0, 0, width, height, (0.7, 0.1, 0.1, 0.2), (0.7, 0.1, 0.1, 0.5))

		for ox in 0:16:width - 16
			Ahorn.drawSprite(ctx, sprite, ox -2, -2, jx=0, jy=0)
		end
end

end