module AdventureHelperDashPushBlock

using ..Ahorn, Maple

@mapdef Entity "AdventureHelper/DashPushBlock" DashPushBlock(x::Integer, y::Integer, width::Integer=16, height::Integer=16)

const placements = Ahorn.PlacementDict(
    "Dash Push Block (Adventure Helper)" => Ahorn.EntityPlacement(
        DashPushBlock,
        "rectangle",
    ),
)

Ahorn.minimumSize(entity::DashPushBlock) = 16, 16
Ahorn.resizable(entity::DashPushBlock) = true, true

function Ahorn.selection(entity::DashPushBlock)
    x, y = Ahorn.position(entity)

    width = Int(get(entity.data, "width", 8))
    height = Int(get(entity.data, "height", 8))

    return [Ahorn.Rectangle(x, y, width, height)]
end

body = "objects/AdventureHelper/dashpushblock/Body"

function Ahorn.renderAbs(ctx::Ahorn.Cairo.CairoContext, entity::DashPushBlock, room::Maple.Room)
    x, y = Ahorn.position(entity)

    width = Int(get(entity.data, "width", 32))
    height = Int(get(entity.data, "height", 32))
	
    tilesWidth = div(width, 8)
    tilesHeight = div(height, 8)

    for i in 2:tilesWidth - 1
        Ahorn.drawImage(ctx, body, x + (i - 1) * 8, y, 8, 0, 8, 8)
        Ahorn.drawImage(ctx, body, x + (i - 1) * 8, y + height - 8, 8, 16, 8, 8)
    end

    for i in 2:tilesHeight - 1
        Ahorn.drawImage(ctx, body, x, y + (i - 1) * 8, 0, 8, 8, 8)
        Ahorn.drawImage(ctx, body, x + width - 8, y + (i - 1) * 8, 16, 8, 8, 8)
    end
	
	for i in 2:tilesWidth - 1
		for j in 2:tilesHeight - 1
			Ahorn.drawImage(ctx, body, x + (i - 1) * 8, y + (j - 1) * 8, 8, 8, 8, 8)
		end
	end

    Ahorn.drawImage(ctx, body, x, y, 0, 0, 8, 8)
    Ahorn.drawImage(ctx, body, x + width - 8, y, 16, 0, 8, 8)
    Ahorn.drawImage(ctx, body, x, y + height - 8, 0, 16, 8, 8)
    Ahorn.drawImage(ctx, body, x + width - 8, y + height - 8, 16, 16, 8, 8)
end

end