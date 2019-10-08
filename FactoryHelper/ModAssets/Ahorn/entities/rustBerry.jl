module FactoryHelperRustBerry

using ..Ahorn, Maple

@mapdef Entity "FactoryHelper/RustBerry" RustBerry(x::Integer, y::Integer)

const placements = Ahorn.PlacementDict(
    "Strawberry (Rusty) (FactoryHelper)" => Ahorn.EntityPlacement(
        RustBerry,
    )
)

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::RustBerry, room::Maple.Room)
    Ahorn.drawSprite(ctx, "objects/FactoryHelper/rustBerry/Berry_01", 0, 0)
    Ahorn.drawSprite(ctx, "objects/FactoryHelper/rustBerry/Gear_01", 0, 0)
end

function Ahorn.selection(entity::RustBerry)
    x, y = Ahorn.position(entity)

    return Ahorn.Rectangle(x - 4, y - 6, 8, 12)
end

end