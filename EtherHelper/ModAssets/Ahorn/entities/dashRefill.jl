module EtherHelperDashRefill

using ..Ahorn, Maple

@mapdef Entity "EtherHelper/DashRefill" DashRefill(x::Integer, y::Integer, amount::Integer=1, offset::Real=0.0, speed::Real=1.0)

const placements = Ahorn.PlacementDict(
    "Dash Refill (EtherHelper)" => Ahorn.EntityPlacement(
        DashRefill,
        "point",
    ),
)

spriteGem = "objects/EtherHelper/dashRefill/gem0"
spriteDanger = "objects/EtherHelper/dashRefill/danger0"


Ahorn.nodeLimits(entity::DashRefill) = 0, -1

function Ahorn.renderAbs(ctx::Ahorn.Cairo.CairoContext, entity::DashRefill, room::Maple.Room)
    x, y = Ahorn.position(entity)
    
    Ahorn.drawSprite(ctx, spriteGem, x, y)
    Ahorn.drawSprite(ctx, spriteDanger, x, y)
end

function Ahorn.renderSelectedAbs(ctx::Ahorn.Cairo.CairoContext, entity::DashRefill)
    px, py = Ahorn.position(entity)

    for node in get(entity.data, "nodes", ())
        nx, ny = Int.(node)

        Ahorn.drawSprite(ctx, spriteGem, nx, ny)
        Ahorn.drawSprite(ctx, spriteDanger, nx, ny)
        Ahorn.drawArrow(ctx, px, py, nx, ny, Ahorn.colors.selection_selected_fc, headLength=6)

        px, py = nx, ny
    end
end

function Ahorn.selection(entity::DashRefill)
    x, y = Ahorn.position(entity)
    nodes = get(entity.data, "nodes", ())
    
    res = Ahorn.Rectangle[Ahorn.Rectangle(x-8,y-8,16,16)]
    
    for node in nodes
        nx, ny = Int.(node)
        push!(res, Ahorn.Rectangle(nx-8,ny-8,16,16))
    end
	
	return res
end

end