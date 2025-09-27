#include <functional>
#include <cstdio>
#include <glm/glm.hpp>

#if defined(_MSC_VER)
    //  Microsoft
#define NAT_API __declspec(dllexport)
#elif defined(__GNUC__)
    //  GCC
#define NAT_API __attribute__((visibility("default")))
#else
    //  do nothing and hope for the best?
#define EXPORT
#define IMPORT
#pragma warning Unknown dynamic link import/export semantics.
#endif

static std::function<void(const char*)> gDebugLog = [](const char* s) { printf("%s\n",s);};

inline static float randf()
{
    return static_cast <float> (rand()) / static_cast <float> (RAND_MAX);
}

// Hash function for pseudo-random values at integer coords
inline float hash(int x, int y) {
    uint32_t h = x * 374761393u + y * 668265263u; // Large primes
    h = (h ^ (h >> 13)) * 1274126177u;
    return (h & 0x00FFFFFF) / float(0x01000000); // [0,1)
}

// Smoothstep (cubic) interpolation
inline float smoothstep(float t) {
    return t * t * (3.0f - 2.0f * t);
}

// Linear interpolation
inline float lerp(float a, float b, float t) {
    return a + t * (b - a);
}

// 2D Value noise
float valueNoise(float x, float y) {
    int xi = (int)std::floor(x);
    int yi = (int)std::floor(y);

    float tx = x - xi;
    float ty = y - yi;

    float u = smoothstep(tx);
    float v = smoothstep(ty);

    // Get values at four corners
    float c00 = hash(xi, yi);
    float c10 = hash(xi + 1, yi);
    float c01 = hash(xi, yi + 1);
    float c11 = hash(xi + 1, yi + 1);

    // Bilinear interpolation
    float nx0 = lerp(c00, c10, u);
    float nx1 = lerp(c01, c11, u);

    return lerp(nx0, nx1, v);
}

extern "C" {

    NAT_API void InitBindings(void(*debugLog)(const char* s))
    {
        gDebugLog = debugLog;
    }

    NAT_API void GenerateTerrain(glm::vec3* vertices, glm::vec3* normals, glm::vec2* uv, int* triangles, int gridSize, float xo, float yo)
    {
        gDebugLog("Generating terrain vertex data...");
        const float denom = 1.0f/(gridSize-1);

        // Random value noise grid starting points every time
        for(int y=0;y<gridSize;++y)
            for (int x = 0; x < gridSize; ++x)
            {
                int o = x + y * gridSize;
                float h = valueNoise(x*0.1f+xo, y*0.1f+yo)*10.0f;
                vertices[o] = glm::vec3(x, h, y);
                uv[o] = glm::vec2(x*denom, y*denom);
            }

        for (int y = 0; y < gridSize; ++y)
            for (int x = 0; x < gridSize; ++x)
            {
                int o = x + y * gridSize;
                const auto& c = vertices[o];
                glm::vec3 normal(0.0f);
                // front-right quadrant
                if(x < (gridSize-1) && y < (gridSize-1))
                    normal += glm::cross(vertices[o + gridSize] - c, vertices[o+1]-c);
                // front-left quadrant
                if (x > 0 && y < (gridSize - 1))
                    normal += glm::cross(vertices[o - 1] - c, vertices[o + gridSize] - c);
                // back-right quadrant
                if (x < (gridSize - 1) && y > 0)
                    normal += glm::cross(vertices[o - gridSize] - c, vertices[o - 1] - c);
                // back-left quadrant
                if (x > 0 && y > 0)
                    normal += glm::cross(vertices[o + 1] - c, vertices[o - gridSize] - c);
                normals[o] = glm::normalize(normal);
            }

        const int numCells = gridSize-1;

        for (int y = 0; y < numCells; ++y)
            for (int x = 0; x < numCells; ++x)
            {
                int o = x+y*gridSize; //vertex index
                int to = (x+y*numCells)*6; // triangle point index (6 per cell)
                triangles[to] = o;
                triangles[to + 1] = o + 1;
                triangles[to + 2] = o + 1 + gridSize;
                triangles[to + 3] = o + 1 + gridSize;
                triangles[to + 4] = o + gridSize;
                triangles[to + 5] = o;
            }
    }
}