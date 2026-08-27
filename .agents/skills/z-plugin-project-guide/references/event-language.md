# Event language and interpreter

## Sources

- Compiler: Assets/Z_Level2/Z_Code/Core/Compile
- Runtime: Assets/Z_Level2/Z_Code/Core/Interpreter.cs
- Safe stored-program update: Assets/Z_Level2/Z_Code/Core/CodeHelper.cs
- Persisted programs: ExtraAssets/*/Core/ef

## Contract

- Statements include assignment, calls, if/else, for, while, break, continue, and Return.
- Any for clause may be empty.
- Operators include arithmetic with modulo, comparisons, unary not, and short-circuit and/or.
- Strings accept single or double quotes plus common and unicode escapes.
- Generate LF-only source with spaces; use GamePause and GameContinue in new code.
- Prefer Compiler.TryCompile and ProgramDataForm.Data.TryApplyCode. Never persist partial output after errors.
- Function arguments bind in source order; expression statements must leave the stack balanced.
- Event-editor Cmd parameter replacement compiles one expression as a temporary terminated statement. Add the missing trailing semicolon only in that single-node editor path; complete stored event programs continue to require explicit statement terminators.
- Recompile each complete ef record from source after code-generation changes. Keep zCode and zCodeMap counts equal.
- `onTileTouchEvent` is a unit-owned scene trigger fired when a moving MapUnit enters a Tile physical Collider. Its heap exposes the moving unit as `self` and the Tile as `target`, both using the existing scene-object handle format; map-edge contact remains `onBoundaryTouchEvent`.

## Limits and validation

The runtime budget is 4096 instructions per top-level call, child depth is 64, and source/AST depth is 256. Build Z_Code, compile the Editor tests, run Tuanjie EditMode tests when licensed, and recompile every persisted ef source.
