module FactoryHelperThrowBox

using ..Ahorn, Maple

@mapdef Entity "FactoryHelper/ThrowBox" ThrowBox(x::Integer, y::Integer, isMetal::Bool=false)

const placements = Ahorn.PlacementDict(
    "ThrowBox (FactoryHelper)" => Ahorn.EntityPlacement(
        ThrowBox,
        "point",
    ),
)

woodSprite = "objects/FactoryHelper/crate/crate0"
metalSprite = "objects/FactoryHelper/crate/crate_metal0"

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

end