#include "pch.h"

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

extern "C" {

    NAT_API void InitBindings(void(*debugLog)(const char* s))
    {
        gDebugLog = debugLog;
    }

    NAT_API void GenerateSomeGeometry(glm::vec3* vertices, glm::vec3* normals, glm::vec2* uv, int maxVerts, int* triangles, int maxTris)
    {
        gDebugLog("Hello from CPP");
    }
}

BOOL APIENTRY DllMain( HMODULE hModule,
                       DWORD  ul_reason_for_call,
                       LPVOID lpReserved
                     )
{
    switch (ul_reason_for_call)
    {
    case DLL_PROCESS_ATTACH:
    case DLL_THREAD_ATTACH:
    case DLL_THREAD_DETACH:
    case DLL_PROCESS_DETACH:
        break;
    }
    return TRUE;
}

