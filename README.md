# Civil3D_Toolkit

Connects Autodesk Civil 3D to the Buildings and Habitats object Model (BHoM). Pull objects from the drawing open in Civil 3D, and push objects back into it, from Grasshopper, Excel or any other BHoM UI.

## Requirements

- Civil 3D 2026
- BHoM 10.0, installed with the BHoM installer

## Getting started

1. Start Civil 3D and open a drawing. The command line shows `BHoM Civil 3D listener started on ports 14230/14231.`
2. In Grasshopper (or Excel), create a `Civil3DAdapter` and set `active` to true.
3. Use **Pull** with a `FilterRequest` to get objects of one type, for example `Line` or `SurveyPoint`. Use a base type such as `ICurve` to get every curve, or leave the request empty to get everything.
4. Use **Push** to add objects to the drawing.

## Civil 3D commands

| Command | What it does |
|---|---|
| `BHOMDEACTIVATE` | Stops the BHoM connection. |
| `BHOMACTIVATE` | Starts the BHoM connection again (or restarts it). |
| `BHOMOUT` | Writes every supported object in the drawing to a JSON file in a `BHoMOut` folder next to the drawing. |

## Supported objects

| BHoM object | Pull from Civil 3D | Push to Civil 3D |
|---|---|---|
| `Point` | Points | Points |
| `Line`, `Arc`, `Circle` | Lines, arcs, circles | Lines, arcs, circles |
| `Ellipse` | Ellipses (elliptical arcs come back as `NurbsCurve`) | Ellipses |
| `PolyCurve` | Polylines (2D and 3D) | Flat curves as polylines at their elevation; other curves as 3D polylines (straight segments only) |
| `NurbsCurve` | Splines | Splines |
| `TextNote` | Text and multiline text | Multiline text |
| `Block` | Block references | Block references (the block definition must already be in the drawing) |
| `BlockDefinition` | Block definitions | — |
| `SurveyPoint` | COGO points | COGO points |
| `FeatureLine` | Feature lines | — |
| `Alignment` | Alignments (spirals come back as straight lines) | — |
| `Parcel` | Parcels | — |

## Good to know

- **Units:** BHoM objects are always in metres. The drawing's units are detected and converted automatically.
- **Layers:** pulled objects remember their layer. When they are pushed back, they go onto the same layer if it exists in the drawing, otherwise onto the current layer. New layers are never created.
- **Points and curves** come back wrapped, so they can carry their layer. Use the wrapper's `WrappedObject` to get the geometry itself. Push the wrapper to keep the layer; push plain geometry to use the current layer.
- **Polyline width:** a curve that carries a `PolylineWidth` is pushed as a polyline of that constant width.

## Not yet supported

These are planned, each with an issue:

- [Gravity and pressure pipe networks (#119)](https://github.com/BHoM/Civil3D_Toolkit/issues/119)
- [TIN surfaces (#120)](https://github.com/BHoM/Civil3D_Toolkit/issues/120)
- [Vertical profiles (#121)](https://github.com/BHoM/Civil3D_Toolkit/issues/121)
- [Infrastructure section definitions (#122)](https://github.com/BHoM/Civil3D_Toolkit/issues/122)
- [Rainbow offset curves (#123)](https://github.com/BHoM/Civil3D_Toolkit/issues/123)
- [Feature line Push (#124)](https://github.com/BHoM/Civil3D_Toolkit/issues/124)
- [Geometry with layer and style (#125)](https://github.com/BHoM/Civil3D_Toolkit/issues/125)
- [Alignment design speeds (#126)](https://github.com/BHoM/Civil3D_Toolkit/issues/126)
- [Block definition Push (#127)](https://github.com/BHoM/Civil3D_Toolkit/issues/127)
- [More Block properties (#128)](https://github.com/BHoM/Civil3D_Toolkit/issues/128)

## More about BHoM

Find out more at [bhom.xyz](https://bhom.xyz).
