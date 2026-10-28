# Autodesk reference assemblies

Reference-only copies used to compile the host-side projects (`Private=False`; they are never deployed).

| Folder | Source | Versions |
|---|---|---|
| `2026/` | `C:\Program Files\Autodesk\AutoCAD 2026\` (+ `ACA\`, `C3D\`) | acdbmgd/acmgd/accoremgd 25.1.164, AecBaseMgd 8.8.58, AeccDbMgd/AeccPressurePipesMgd 13.8.946 |
| `2019/` | Archive of the original references. Not used by any build configuration. | acdbmgd 23.0.162, AeccDbMgd 13.0.1361, AecBaseMgd 8.1.48 |

To add a year: create `libs/<year>/` with the same six files from that year's install and add the configuration (see `Civil3D_Core_*/*.csproj`).
