# BuildingSystem

## Controls
* **LMB**: Build / Edit (drag existing edge).
* **LMB + Shift**(hold): Delete (Eraser).

### Visual Cues (Cursor):
* **White**: Idle / Build.
* **Blue**: Edit.
* **Red**: Delete.

## Technical Implementation
* **Data-Oriented Design**: Core logic is built using Unity Job System and Burst.
* **Rendering**: Built on `Graphics.RenderMeshInstanced`. Animation metadata (Spawn/Death time) is passed via unused matrix cells (m30, m31, m32).
* **Memory Management**: Background **Compaction** process to handle memory fragmentation.
* **Spatial Grid**: Custom 2D spatial hash for $O(1)$ object lookup and overlap validation (no Unity Physics used, almost).

Recommended starting point for code research:
**`MainGameController.cs`**: Entry point.