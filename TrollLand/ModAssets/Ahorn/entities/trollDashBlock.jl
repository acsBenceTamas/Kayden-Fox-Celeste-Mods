module TrollLandTrollDashBlock

using ..Ahorn, Maple

@mapdef Entity "TrollLand/TrollDashBlock" TrollDashBlock(x::Integer, y::Integer, width::Integer=8, height::Integer=8, tiletype::String="3", blendin::Bool=true, permanent::Bool=false, forward::Bool=false)

const placements = Ahorn.PlacementDict(
    "Troll Dash Block (Troll Land)" => Ahorn.EntityPlacement(
        TrollDashBlock,
        "rectangle",
        Dict{String, Any}(),
        Ahorn.tileEntityFinalizer
    )
)

Ahorn.editingOptions(entity::TrollDashBlock) = Dict{String, Any}(
    "tiletype" => Ahorn.tiletypeEditingOptions()
)

Ahorn.minimumSize(entity::TrollDashBlock) = 8, 8
Ahorn.resizable(entity::TrollDashBlock) = true, true

Ahorn.selection(entity::TrollDashBlock) = Ahorn.getEntityRectangle(entity)

Ahorn.renderAbs(ctx::Ahorn.Cairo.CairoContext, entity::TrollDashBlock, room::Maple.Room) = Ahorn.drawTileEntity(ctx, room, entity)

end