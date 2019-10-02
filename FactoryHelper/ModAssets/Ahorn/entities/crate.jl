module FactoryHelperThrowBox

using ..Ahorn, Maple

@mapdef Entity "FactoryHelper/ThrowBox" ThrowBox(x::Integer, y::Integer, isMetal::Bool=false)
@mapdef Entity "FactoryHelper/ThrowBoxSpawner" ThrowBoxSpawner(x::Integer, y::Integer, delay::Real=1.0, maximum::Integer=0, isMetal::Bool=false, isRandom::Bool=false, fromTop::Bool=true)

const placements = Ahorn.PlacementDict(
    "ThrowBox (FactoryHelper)" => Ahorn.EntityPlacement(
        ThrowBox,
        "point",
    ),
    "ThrowBoxSpawner (FactoryHelper)" => Ahorn.EntityPlacement(
        ThrowBoxSpawner,
        "point",
    ),
)

woodSprite = "objects/FactoryHelper/crate/crate0"
metalSprite = "objects/FactoryHelper/crate/crate_metal0"
normalColor = (1.0, 1.0, 1.0, 0.0)
randomColor = (1.0, 0.5, 1.0, 0.0)

function Ahorn.renderAbs(ctx::Ahorn.Cairo.CairoContext, entity::ThrowBox, room::Maple.Room)
    x, y = Ahorn.position(entity)
    sprite = String
    
    if get(entity.data, "isMetal", false)
        sprite = metalSprite
    else
        sprite = woodSprite
    end
    
    Ahorn.drawSprite(ctx, sprite, x + 8, y + 8)
end

function Ahorn.renderSelectedAbs(ctx::Ahorn.Cairo.CairoContext, entity::ThrowBox)
end

function Ahorn.selection(entity::ThrowBox)
    x, y = Ahorn.position(entity)
    
    return Ahorn.Rectangle[Ahorn.Rectangle(x,y,16,16)]
end

function Ahorn.renderAbs(ctx::Ahorn.Cairo.CairoContext, entity::ThrowBoxSpawner, room::Maple.Room)
    x, y = Ahorn.position(entity)
    sprite = String
    
    if get(entity.data, "isMetal", false)
        sprite = metalSprite
    else
        sprite = woodSprite
    end
	
	color = Tuple{Real,Real,Real,Real}
    
    if get(entity.data, "isRandom", false)
        color = randomColor
    else
        color = normalColor
    end
    
    Ahorn.drawSprite(ctx, sprite, x + 8, y + 8)
    Ahorn.drawRectangle(ctx, x, y, 16, 16, color .+ (0.0, 0.0, 0.0, 0.2), color .+ (0.0, 0.0, 0.0, 0.5))
	if get(entity.data, "fromTop", true)
		Ahorn.drawArrow(ctx, x+8, y+4, x+8, y+12, (0.0, 0.0, 0.7, 1.0), headLength=3)
	end
end

function Ahorn.renderSelectedAbs(ctx::Ahorn.Cairo.CairoContext, entity::ThrowBoxSpawner)
end

function Ahorn.selection(entity::ThrowBoxSpawner)
    x, y = Ahorn.position(entity)
    
    return Ahorn.Rectangle[Ahorn.Rectangle(x,y,16,16)]
end

end