module TrollLandTrollFakeWall

using ..Ahorn, Maple

@mapdef Entity "TrollLand/TrollFakeWall" TrollFakeWall(x::Integer, y::Integer, width::Integer=8, height::Integer=8, tiletype::String="3")

const placements = Ahorn.PlacementDict(
    "Troll Fake Wall (Troll Land)" => Ahorn.EntityPlacement(
        TrollFakeWall,
        "rectangle",
        Dict{String, Any}(),
        Ahorn.tileEntityFinalizer
    ),
)

Ahorn.editingOptions(entity::TrollFakeWall) = Dict{String, Any}(
    "tiletype" => Ahorn.tiletypeEditingOptions()
)

Ahorn.minimumSize(entity::TrollFakeWall) = 8, 8
Ahorn.resizable(entity::TrollFakeWall) = true, true

Ahorn.selection(entity::TrollFakeWall) = Ahorn.getEntityRectangle(entity)

Ahorn.renderAbs(ctx::Ahorn.Cairo.CairoContext, entity::TrollFakeWall, room::Maple.Room) = Ahorn.drawTileEntity(ctx, room, entity, alpha=0.7)

end