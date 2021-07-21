module AdventureHelperGroupedFallingBlock

using ..Ahorn, Maple

@mapdef Entity "AdventureHelper/GroupedFallingBlock" GroupedFallingBlock(x::Integer, y::Integer, width::Integer=16, height::Integer=16, tiletype::String="3", climbFall::Bool=true )

const placements = Ahorn.PlacementDict(
    "Falling Block (Grouped) (Adventure Helper)" => Ahorn.EntityPlacement(
        GroupedFallingBlock,
        "rectangle",
        Dict{String, Any}(),
        Ahorn.tileEntityFinalizer
    )
)

Ahorn.minimumSize(entity::GroupedFallingBlock) = 8, 8
Ahorn.resizable(entity::GroupedFallingBlock) = true, true

Ahorn.selection(entity::GroupedFallingBlock) = Ahorn.getEntityRectangle(entity)

Ahorn.renderAbs(ctx::Ahorn.Cairo.CairoContext, entity::GroupedFallingBlock, room::Maple.Room) = Ahorn.drawTileEntity(ctx, room, entity)

end