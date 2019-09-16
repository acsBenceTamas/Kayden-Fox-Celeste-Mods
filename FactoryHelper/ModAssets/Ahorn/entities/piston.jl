module FactoryHelperPiston

using ..Ahorn, Maple

@mapdef Entity "FactoryHelper/Piston" Piston(x::Integer, y::Integer, nodes::Array{Tuple{Integer, Integer}, 1}=Tuple{Integer, Integer}[])

const placements = Ahorn.PlacementDict(
    "Piston (Up) (FactoryHelper)" => Ahorn.EntityPlacement(
        Piston,
        "point",
        Dict{String, Any}(),
        function(entity)
            # entity.data["nodes"] = [(Int(entity.data["x"]), Int(entity.data["y"]) + 16), (Int(entity.data["x"]), Int(entity.data["y"]) + 32)]
        end
    ),
)

Ahorn.nodeLimits(entity::Piston) = 2, 2
	
baseSprite = "objects/FactoryHelper/piston/base"
headSprite = "objects/FactoryHelper/piston/head"

function Ahorn.renderAbs(ctx::Ahorn.Cairo.CairoContext, entity::Piston, room::Maple.Room)
    x, y = Ahorn.position(entity)
    # sx, sy = Int.(entity.data["nodes"][1])
    # ex, ey = Int.(entity.data["nodes"][2])
	
    Ahorn.drawSprite(ctx, baseSprite, x, y)
    # Ahorn.drawSprite(ctx, headSprite, sx, sy)
    # Ahorn.drawSprite(ctx, headSprite, ex, ey)
end


function Ahorn.renderSelectedAbs(ctx::Ahorn.Cairo.CairoContext, entity::Piston)
    x, y = Ahorn.position(entity)
    # sx, sy = Int.(entity.data["nodes"][1])
    # ex, ey = Int.(entity.data["nodes"][2])
	
    Ahorn.drawSprite(ctx, baseSprite, x, y)
    # Ahorn.drawSprite(ctx, headSprite, sx, sy)
    # Ahorn.drawSprite(ctx, headSprite, ex, ey)
	
	# Ahorn.drawArrow(ctx, sx, sy, ex, ey, Ahorn.colors.selection_selected_fc, headLength=6)
end

function Ahorn.selection(entity::Piston)
    # nodes = get(entity.data, "nodes", ())
    x, y = Ahorn.position(entity)

    res = Ahorn.Rectangle[Ahorn.getSpriteRectangle(baseSprite, x, y)]
    
    #for node in nodes
    #    nx, ny = Int.(node)
#
#        push!(res, Ahorn.getSpriteRectangle(headSprite, nx, ny))
#    end

    return res
end

end