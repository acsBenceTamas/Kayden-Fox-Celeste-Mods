module FactoryHelperRustyMetalJumpthru

using ..Ahorn, Maple

@mapdef Entity "FactoryHelper/RustyJumpthruPlatform" RustyMetalJumpthru(x::Integer, y::Integer, width::Integer=8)

const placements = Ahorn.PlacementDict(
    "Jump Through (Rusty Metal) (FactoryHelper)" => Ahorn.EntityPlacement(
        RustyMetalJumpthru,
        "rectangle",
    )
)

quads = Tuple{Integer, Integer, Integer, Integer}[
    (0, 0, 8, 7) (8, 0, 8, 7) (16, 0, 8, 7);
    (0, 8, 8, 5) (8, 8, 8, 5) (16, 8, 8, 5)
]

Ahorn.minimumSize(entity::RustyMetalJumpthru) = 8, 0
Ahorn.resizable(entity::RustyMetalJumpthru) = true, false

function Ahorn.selection(entity::RustyMetalJumpthru)
    x, y = Ahorn.position(entity)
    width = Int(get(entity.data, "width", 8))

    return Ahorn.Rectangle(x, y, width, 8)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::RustyMetalJumpthru, room::Maple.Room)

    x = Int(get(entity.data, "x", 0))
    y = Int(get(entity.data, "y", 0))

    width = Int(get(entity.data, "width", 8))

    startX = div(x, 8) + 1
    stopX = startX + div(width, 8) - 1
    startY = div(y, 8) + 1

    len = stopX - startX
    for i in 0:len
        connected = false
        qx = 2
        if i == 0
            connected = get(room.fgTiles.data, (startY, startX - 1), false) != '0'
            qx = 1

        elseif i == len
            connected = get(room.fgTiles.data, (startY, stopX + 1), false) != '0'
            qx = 3
        end

        quad = quads[2 - connected, qx]
        Ahorn.drawImage(ctx, "objects/FactoryHelper/jumpThru/rustyMetal", 8 * i, 0, quad...)
    end
end

end