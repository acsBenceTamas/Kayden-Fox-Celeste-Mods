module TrollLandTrollWall

using ..Ahorn, Maple

@mapdef Entity "TrollLand/TrollWall" TrollWall(x::Integer, y::Integer, width::Integer=8, height::Integer=8, tiletype::String="3", maxSpeed::Real=30.0, idString::String="", doPause::Bool=true, underFg::Bool=false)

const placements = Ahorn.PlacementDict(
    "Troll Wall (Troll Land)" => Ahorn.EntityPlacement(
        TrollWall,
        "rectangle",
        Dict{String, Any}(),
        function(entity)
            entity.data["nodes"] = [(Int(entity.data["x"]) + Int(entity.data["width"]) + 8, Int(entity.data["y"]))]
            Ahorn.tileEntityFinalizer(entity)
        end,
    )
)

Ahorn.editingOptions(entity::TrollWall) = Dict{String, Any}(
    "tiletype" => Ahorn.tiletypeEditingOptions()
)

Ahorn.minimumSize(entity::TrollWall) = 8, 8
Ahorn.resizable(entity::TrollWall) = true, true
Ahorn.nodeLimits(entity::TrollWall) = 1, -1

function Ahorn.selection(entity::TrollWall)
    nodes = get(entity.data, "nodes", ())
    x, y = Ahorn.position(entity)

    width = Int(get(entity.data, "width", 8))
    height = Int(get(entity.data, "height", 8))

    res = Ahorn.Rectangle[Ahorn.getEntityRectangle(entity)]
    
    for node in nodes
        nx, ny = Int.(node)

        push!(res, Ahorn.Rectangle(nx, ny, width, height))
    end

    return res
end

function Ahorn.renderAbs(ctx::Ahorn.Cairo.CairoContext, entity::TrollWall, room::Maple.Room)
	Ahorn.drawTileEntity(ctx, room, entity)
end

function Ahorn.renderSelectedAbs(ctx::Ahorn.Cairo.CairoContext, entity::TrollWall, room::Maple.Room)
    px, py = Ahorn.position(entity)
	material = get(entity.data, "tiletype", "3")[1]

    width = Int(get(entity.data, "width", 8))
    height = Int(get(entity.data, "height", 8))

    for node in get(entity.data, "nodes", ())
        nx, ny = Int.(node)

        fakeTiles = Ahorn.createFakeTiles(room, nx, ny, width, height, material, blendIn=false)
        Ahorn.drawFakeTiles(ctx, room, fakeTiles, room.objTiles, true, nx, ny, clipEdges=true)
		
        Ahorn.drawArrow(ctx, px + width / 2 , py + height / 2, nx + width / 2 , ny + height / 2, Ahorn.colors.selection_selected_fc, headLength=6)

        px, py = nx, ny
	end
end

end