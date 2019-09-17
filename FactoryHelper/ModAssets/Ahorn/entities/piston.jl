module FactoryHelperPiston

using ..Ahorn, Maple

@mapdef Entity "FactoryHelper/Piston" Piston(x::Integer, y::Integer, direction::String, moveTime::Real=0.4, pauseTime::Real=0.2)

const placements = Ahorn.PlacementDict(
    "Piston (Up) (FactoryHelper)" => Ahorn.EntityPlacement(
        Piston,
        "point",
        Dict{String, Any}(),
        function(entity)
            entity.data["nodes"] = [(Int(entity.data["x"]), Int(entity.data["y"]) - 16), (Int(entity.data["x"]), Int(entity.data["y"]) - 32)]
			entity.data["direction"] = "Up"
			entity.data["y"] = entity.data["y"] + 4
        end
    ),
)

Ahorn.nodeLimits(entity::Piston) = 2, 2
	
baseSprite = "objects/FactoryHelper/piston/base00"
headSprite = "objects/FactoryHelper/piston/head00"

spritePostfixes = Dict{String, String}(
	"Up" => "u",
	"Down" => "d",
	"Left" => "l",
	"Right" => "r"
)

function Ahorn.renderAbs(ctx::Ahorn.Cairo.CairoContext, entity::Piston, room::Maple.Room)
    x, y = Ahorn.position(entity)
    sx, sy = Int.(entity.data["nodes"][1])
    ex, ey = Int.(entity.data["nodes"][2])
	y = y + 4
	sy = sy + 4
	ey = ey + 4
	x = x + 8
	sx = sx + 8
	ex = ex + 8
	
	
	Ahorn.drawRectangle(ctx, x-4, ey, 8, y-ey, (0.4, 0.3, 0.2, 0.4), (0.4, 0.3, 0.2, 1.0))
	Ahorn.drawRectangle(ctx, x-4, ey, 8, sy-ey, (0.4, 0.3, 0.2, 0.4), (0.4, 0.3, 0.2, 1.0))
    Ahorn.drawSprite(ctx, baseSprite, x, y)
    Ahorn.drawSprite(ctx, headSprite, sx, sy)
    Ahorn.drawSprite(ctx, headSprite, ex, ey)
end


function Ahorn.renderSelectedAbs(ctx::Ahorn.Cairo.CairoContext, entity::Piston)
    sx, sy = Int.(entity.data["nodes"][1])
    ex, ey = Int.(entity.data["nodes"][2])
	sy = sy + 4
	ey = ey + 4
	ex = ex + 8
	sx = sx + 8
	
	Ahorn.drawArrow(ctx, sx, sy, ex, ey, Ahorn.colors.selection_selected_fc, headLength=6)
end

function Ahorn.selection(entity::Piston)
    nodes = get(entity.data, "nodes", ())
    x, y = Ahorn.position(entity)
	y = y + 4
	x = x + 8

    res = Ahorn.Rectangle[Ahorn.getSpriteRectangle(baseSprite, x, y)]
    
    for node in nodes
        nx, ny = Int.(node)
		ny = ny + 4
		nx = nx + 8

        push!(res, Ahorn.getSpriteRectangle(headSprite, nx, ny))
    end

    return res
end

end