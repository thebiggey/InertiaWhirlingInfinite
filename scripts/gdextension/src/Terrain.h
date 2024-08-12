#ifndef TERRAIN_H
#define TERRAIN_H


#indclude <godot_cpp/classes/node3d.hpp>

namespace godot {

    class Terrain : public Node3D {
        GDCLASS(Terrain, Node3D)

    private:
        double radius;

    }
}
