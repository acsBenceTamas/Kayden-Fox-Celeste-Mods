module FactoryHelperBoomBox

using ..Ahorn, Maple

@mapdef Entity "FactoryHelper/BoomBox" BoomBox(x::Integer, y::Integer, activationId::String="", initialDelay::Real=0.0, startActive::Bool=false)

const placements = Ahorn.PlacementDict(
    "BoomBox (FactoryHelper)" => Ahorn.EntityPlacement(
        BoomBox,
        "point",
    ),
)

idleSprite = "objects/FactoryHelper/boomBox/idle00"
activeSprite = "objects/FactoryHelper/boomBox/active00"

Ahorn.nodeLimits(entity::BoomBox) = 0, 0

function Ahorn.renderAbs(ctx::Ahorn.Cairo.CairoContext, entity::BoomBox, room::Maple.Room)
    x, y = Ahorn.position(entity)
    sprite = String
    
    if get(entity.data, "startActive", false)
        sprite = activeSprite
    else
        sprite = idleSprite
    end
    
    Ahorn.drawSprite(ctx, sprite, x + 12, y + 12)
end

function Ahorn.renderSelectedAbs(ctx::Ahorn.Cairo.CairoContext, entity::BoomBox)
end

function Ahorn.selection(entity::BoomBox)
    x, y = Ahorn.position(entity)
    
    return Ahorn.Rectangle[Ahorn.Rectangle(x,y,24,24)]
end

end