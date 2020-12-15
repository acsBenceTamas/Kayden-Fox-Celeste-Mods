module FactoryHelperFactoryActivatorDashBlock

using ..Ahorn, Maple

@mapdef Entity "FactoryHelper/FactoryActivatorDashBlock" FactoryActivatorDashBlock(x::Integer, y::Integer, width::Integer=8, height::Integer=8, tiletype::String="3", blendin::Bool=true, canDash::Bool=true, permanent::Bool=true, activationIds::String="")

const placements = Ahorn.PlacementDict(
    "Dash Block - Factory Activator (Factory Helper)" => Ahorn.EntityPlacement(
        FactoryActivatorDashBlock,
        "rectangle",
        Dict{String, Any}(),
        Ahorn.tileEntityFinalizer
    )
)

Ahorn.editingOptions(entity::FactoryActivatorDashBlock) = Dict{String, Any}(
    "tiletype" => Ahorn.tiletypeEditingOptions()
)

Ahorn.minimumSize(entity::FactoryActivatorDashBlock) = 8, 8
Ahorn.resizable(entity::FactoryActivatorDashBlock) = true, true

Ahorn.selection(entity::FactoryActivatorDashBlock) = Ahorn.getEntityRectangle(entity)

Ahorn.renderAbs(ctx::Ahorn.Cairo.CairoContext, entity::FactoryActivatorDashBlock, room::Maple.Room) = Ahorn.drawTileEntity(ctx, room, entity)

end