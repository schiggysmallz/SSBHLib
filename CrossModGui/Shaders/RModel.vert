#version 330

in vec4 Position0;
in vec4 Tangent0;
in vec4 Normal0;

in vec4 colorSet1;
in uvec4 colorSet2Packed;
in uvec4 colorSet3456Packed;
in vec4 colorSet7;

in vec2 map1;
in vec4 uvSetUvSet1;
in vec4 uvSet2Bake1;

in ivec4 boneIndices;
in vec4 boneWeights;

out vec3 geomVertexNormal;
out vec4 geomTangent;
out vec2 geomMap1;
out vec2 geomUvSet;
out vec2 geomUvSet1;
out vec2 geomUvSet2;
out vec4 geomColorSet1;
out vec4 geomColorSet2;
out vec4 geomColorSet2_1;
out vec4 geomColorSet2_2;
out vec4 geomColorSet2_3;
out vec4 geomColorSet3;
out vec4 geomColorSet4;
out vec4 geomColorSet5;
out vec4 geomColorSet6;
out vec4 geomColorSet7;
out vec2 geomBake1;
out vec3 geomPosition;

uniform mat4 mvp;
uniform mat4 modelView;
uniform mat4 transform;

uniform Bones
{
    mat4 transforms[300];
};

uniform MaterialParams
{
    vec4 CustomVector0;
    vec4 CustomVector3;
    vec4 CustomVector6;
    vec4 CustomVector8;
    vec4 CustomVector11;
    vec4 CustomVector13;
    vec4 CustomVector14;
    vec3 CustomVector18;
    vec4 CustomVector30;
    vec4 CustomVector31;
    vec4 CustomVector32;
    vec4 CustomVector42;
    vec4 CustomVector47;
    vec4 CustomVector44;
    vec4 CustomVector45;

    vec4 vec4Param;

    int CustomBoolean1;
    int CustomBoolean2;
    int CustomBoolean3;
    int CustomBoolean4;
    int CustomBoolean9;
    int CustomBoolean11;

    float CustomFloat1;
    float CustomFloat4;
    float CustomFloat8;
    float CustomFloat10;
    float CustomFloat19;

    int hasCustomVector11;
    int hasCustomVector47;
    int hasCustomVector44;
    int hasCustomFloat10;
    int hasCustomBoolean1;

    int hasColMap; 
    int hasCol2Map; 
    int hasInkNorMap; 
    int hasDifCubeMap; 
    int hasDiffuse; 
    int hasDiffuse2;
    int hasDiffuse3; 
    int emissionOverride;
};

vec4 UnpackVec4(uint val) 
{
    // RGBA 32 bits
    // TODO: Increase the OpenGL version to 4.0 and just unpackUnorm4x8.
    return vec4(((val >> 24) & 0xFFu) / 255.0, ((val >> 16) & 0xFFu) / 255.0, ((val >> 8) & 0xFFu) / 255.0, ((val >> 0) & 0xFFu) / 255.0);
}

void main()
{
    // Single bind transform
    vec4 position = transform * vec4(Position0.xyz, 1);
    vec4 transformedNormal = transform * vec4(Normal0.xyz, 0);
    vec4 transformedTangent = transform * vec4(Tangent0.xyz, 0);

    // Vertex skinning
    if (boneWeights.x != 0) {
        position = vec4(0);
        transformedNormal = vec4(0);

        for (int i = 0; i < 4; i++)
        {
            position += transforms[boneIndices[i]] * vec4(Position0.xyz, 1) * boneWeights[i];
            transformedNormal.xyz += (inverse(transpose(transforms[boneIndices[i]])) * vec4(Normal0.xyz, 1) * boneWeights[i]).xyz;
            transformedTangent.xyz += (inverse(transpose(transforms[boneIndices[i]])) * vec4(Tangent0.xyz, 1) * boneWeights[i]).xyz;
        }
    }

    transformedNormal.xyz = normalize(transformedNormal.xyz);
    transformedTangent.xyz = normalize(transformedTangent.xyz);

    // Assign geometry inputs
    geomVertexNormal = transformedNormal.xyz;
    geomColorSet1 = colorSet1 / 128.0;
    geomColorSet7 = colorSet7 / 128.0;

    // Pack colors together to avoid hitting hardware limits of 16 attributes for some vendors.
    geomColorSet2 = UnpackVec4(colorSet2Packed.x) * 2;
    geomColorSet2_1 = UnpackVec4(colorSet2Packed.y) * 2;
    geomColorSet2_2 = UnpackVec4(colorSet2Packed.z) * 2;
    geomColorSet2_3 = UnpackVec4(colorSet2Packed.w) * 2;
    geomColorSet3 = UnpackVec4(colorSet3456Packed.x) * 2;
    geomColorSet4 = UnpackVec4(colorSet3456Packed.y) * 2;
    geomColorSet5 = UnpackVec4(colorSet3456Packed.z) * 2;
    geomColorSet6 = UnpackVec4(colorSet3456Packed.w) * 2;

    geomPosition = position.xyz;

    // Sprite sheet uvs.
    geomMap1 = map1;
    if (CustomBoolean9 == 1)
        geomMap1 /= CustomVector18.xy;

    // Pack uv sets together to save on attributes.
    geomUvSet = uvSetUvSet1.xy;
    geomUvSet1 = uvSetUvSet1.zw;
    geomUvSet2 = uvSet2Bake1.xy;
    geomBake1 = uvSet2Bake1.zw;

    // The w component flips mirrored tangents.
    geomTangent = vec4(transformedTangent.xyz, Tangent0.w);
    gl_Position = mvp * vec4(position.xyz, 1);
}
