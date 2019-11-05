module FactoryHelperPowerLine

using ..Ahorn, Maple

@mapdef Entity "FactoryHelper/PowerLine" PowerLine(x::Integer, y::Integer, activationId::String="", colorCode::String="00dd00", startActive::Bool=false, initialDelay::Real=0.0)

const placements = Ahorn.PlacementDict(
    "Power Line (FactoryHelper)" => Ahorn.EntityPlacement(
        PowerLine,
        "point",
        Dict{String, Any}(),
        function(entity)
            entity.data["nodes"] = [(Int(entity.data["x"]) + 16, Int(entity.data["y"]))]
        end
    )
)

sprite = "objects/FactoryHelper/powerLine/powerLine_c"
color = (0.7, 0.7, 0.7, 1.0)
activeColor = (0.2, 0.7, 0.2, 1.0)

Ahorn.nodeLimits(entity::PowerLine) = 1, -1

function Ahorn.renderAbs(ctx::Ahorn.Cairo.CairoContext, entity::PowerLine, room::Maple.Room)
    x, y = Ahorn.position(entity)
    nodes = get(entity.data, "nodes", ())
    trueColor = Tuple{Real,Real,Real,Real}
    
    if get(entity.data, "startActive", false)
        trueColor = activeColor
    else
        trueColor = color
    end
    
    Ahorn.drawSprite(ctx, sprite, x+4, y+4)

    px, py = x, y
    for node in nodes
        nx, ny = Int.(node)

        Ahorn.drawSprite(ctx, sprite, nx+4, ny+4)
        Ahorn.drawArrow(ctx, px+4, py+4, nx+4, ny+4, trueColor, headLength=0)

        px, py = nx, ny
    end
end

function Ahorn.renderSelectedAbs(ctx::Ahorn.Cairo.CairoContext, entity::PowerLine)
    px, py = Ahorn.position(entity)

    for node in get(entity.data, "nodes", ())
        nx, ny = Int.(node)

        Ahorn.drawArrow(ctx, px+4, py+4, nx+4, ny+4, Ahorn.colors.selection_selected_fc, headLength=0)

        px, py = nx, ny
    end
end

function Ahorn.selection(entity::PowerLine)
    x, y = Ahorn.position(entity)
    nodes = get(entity.data, "nodes", ())
    
    res = Ahorn.Rectangle[Ahorn.Rectangle(x,y,8,8)]
    
    for node in nodes
        nx, ny = Int.(node)
        push!(res, Ahorn.Rectangle(nx,ny,8,8))
    end

    return res
end

end