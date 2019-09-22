module FactoryHelperPiston

using ..Ahorn, Maple

@mapdef Entity "FactoryHelper/PistonUp" PistonUp(x::Integer, y::Integer, moveTime::Real=0.4, pauseTime::Real=0.2, initialDelay::Real=0.0, startActive::Bool=true, heated::Bool=false, activationId::String="")
@mapdef Entity "FactoryHelper/PistonDown" PistonDown(x::Integer, y::Integer, moveTime::Real=0.4, pauseTime::Real=0.2, initialDelay::Real=0.0, startActive::Bool=true, heated::Bool=false, activationId::String="")
@mapdef Entity "FactoryHelper/PistonLeft" PistonLeft(x::Integer, y::Integer, moveTime::Real=0.4, pauseTime::Real=0.2, initialDelay::Real=0.0, startActive::Bool=true, heated::Bool=false, activationId::String="")
@mapdef Entity "FactoryHelper/PistonRight" PistonRight(x::Integer, y::Integer, moveTime::Real=0.4, pauseTime::Real=0.2, initialDelay::Real=0.0, startActive::Bool=true, heated::Bool=false, activationId::String="")

pistonUnion = Union{PistonUp, PistonDown, PistonLeft, PistonRight}

const placements = Ahorn.PlacementDict(
    "Piston (Up) (FactoryHelper)" => Ahorn.EntityPlacement(
        PistonUp,
        "point",
        Dict{String, Any}(),
        function(entity)
            entity.data["nodes"] = [(Int(entity.data["x"]), Int(entity.data["y"]) - 32), (Int(entity.data["x"]), Int(entity.data["y"]) - 16)]
        end
    ),
    "Piston (Down) (FactoryHelper)" => Ahorn.EntityPlacement(
        PistonDown,
        "point",
        Dict{String, Any}(),
        function(entity)
            entity.data["nodes"] = [(Int(entity.data["x"]), Int(entity.data["y"]) + 32), (Int(entity.data["x"]), Int(entity.data["y"]) + 16)]
        end
    ),
    "Piston (Left) (FactoryHelper)" => Ahorn.EntityPlacement(
        PistonLeft,
        "point",
        Dict{String, Any}(),
        function(entity)
            entity.data["nodes"] = [(Int(entity.data["x"]) - 32, Int(entity.data["y"])), (Int(entity.data["x"]) - 16, Int(entity.data["y"]))]
        end
    ),
    "Piston (Right) (FactoryHelper)" => Ahorn.EntityPlacement(
        PistonRight,
        "point",
        Dict{String, Any}(),
        function(entity)
            entity.data["nodes"] = [(Int(entity.data["x"]) + 32, Int(entity.data["y"])), (Int(entity.data["x"]) + 16, Int(entity.data["y"]))]
        end
    ),
)

Ahorn.nodeLimits(entity::pistonUnion) = 2, 2
	
baseSprite = "objects/FactoryHelper/piston/base00"
headSprite = "objects/FactoryHelper/piston/head00"

directions = Dict{String, String}(
    "FactoryHelper/PistonUp" => "Up",
    "FactoryHelper/PistonDown" => "Down",
    "FactoryHelper/PistonLeft" => "Left",
    "FactoryHelper/PistonRight" => "Right",
)

directionRotation = Dict{String,Real}(
	"Up" => 0,
	"Down" => pi,
	"Left" => -pi/2,
	"Right" => pi/2,
)

directionDisplacement = Dict{String,Tuple{Integer,Integer}}(
	"Up" => (0,0),
	"Down" => (16,8),
	"Left" => (0,16),
	"Right" => (8,0),
)

function Ahorn.renderAbs(ctx::Ahorn.Cairo.CairoContext, entity::pistonUnion, room::Maple.Room)
    x, y = Ahorn.position(entity)
    sx, sy = Int.(entity.data["nodes"][1])
    ex, ey = Int.(entity.data["nodes"][2])
	direction = get(directions, entity.name, "Up")
	
	y = y + 4
	sy = sy + 4
	ey = ey + 4
	x = x + 8
	sx = sx + 8
	ex = ex + 8
	
	rectangleColor1 = Tuple{Real,Real,Real,Real}
	rectangleColor2 = Tuple{Real,Real,Real,Real}
	
	displaceSpriteX = directionDisplacement[direction][1]
	displaceSpriteY = directionDisplacement[direction][2]
	
	if get(entity.data, "heated", false) == true
		rectangleColor1 = (0.5, 0.1, 0.0, 0.4)
		rectangleColor2 = (0.5, 0.1, 0.0, 1.0)
	else
		rectangleColor1 = (0.4, 0.3, 0.2, 0.4)
		rectangleColor2 = (0.4, 0.3, 0.2, 1.0)
	end
	
	if direction == "Up" || direction == "Down"
		Ahorn.drawRectangle(ctx, x-4, ey, 8, y-ey, rectangleColor1, rectangleColor2)
		Ahorn.drawRectangle(ctx, x-4, ey, 8, sy-ey, rectangleColor1, rectangleColor2)
	else
		Ahorn.drawRectangle(ctx, ex-4, y, x-ex, 8, rectangleColor1, rectangleColor2)
		Ahorn.drawRectangle(ctx, ex-4, y, sx-ex, 8, rectangleColor1, rectangleColor2)
	end
    Ahorn.drawSprite(ctx, baseSprite, x + displaceSpriteX, y + displaceSpriteY, rot=directionRotation[direction])
    Ahorn.drawSprite(ctx, headSprite, sx + displaceSpriteX, sy + displaceSpriteY, rot=directionRotation[direction])
    Ahorn.drawSprite(ctx, headSprite, ex + displaceSpriteX, ey + displaceSpriteY, rot=directionRotation[direction])
	
	if direction == "Left" || direction == "Right"
		sy = sy + 4
		ey = ey + 4
		ex = ex - 4
		sx = sx - 4
	end
	Ahorn.drawArrow(ctx, sx, sy, ex, ey, (0.0, 0.0, 0.7, 1.0), headLength=4)
end


function Ahorn.renderSelectedAbs(ctx::Ahorn.Cairo.CairoContext, entity::pistonUnion)
end

function Ahorn.selection(entity::pistonUnion)
    nodes = get(entity.data, "nodes", ())
    x, y = Ahorn.position(entity)
	direction = get(directions, entity.name, "Up")
	
	y = y + 4
	x = x + 8
	
	res = Ahorn.Rectangle[]
	
	if direction == "Up" || direction == "Down"
		push!(res, Ahorn.getSpriteRectangle(baseSprite, x, y))
	else
		push!(res, Ahorn.Rectangle(x-8,y - 4,8,16))
	end
    
    for node in nodes
        nx, ny = Int.(node)
		ny = ny + 4
		nx = nx + 8

		if direction == "Up" || direction == "Down"
			push!(res, Ahorn.getSpriteRectangle(baseSprite, nx, ny))
		else
			push!(res, Ahorn.Rectangle(nx-8,ny - 4,8,16))
		end
    end

    return res
end

end