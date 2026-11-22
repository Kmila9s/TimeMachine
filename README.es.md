<p align="right"><a href="README.md">English</a></p>

<p align="center"><img src="docs/assets/banner-es.png" alt="TimeMachine — Base de tiempo discreta: ticks, relojes y eventos" width="100%"></p>

<p align="center"><a href="LICENSE"><img src="https://img.shields.io/badge/license-MIT-3c82f6" alt="License: MIT"></a> <a href="https://dotnet.microsoft.com/"><img src="https://img.shields.io/badge/.NET-10-475569" alt=".NET 10"></a> <a href="https://kmila.com.mx"><img src="https://img.shields.io/badge/HDL-VHDL-475569" alt="VHDL"></a> <a href="https://github.com/Kmila9s/TimeMachine/releases"><img src="https://img.shields.io/github/v/release/Kmila9s/TimeMachine?color=3c82f6&label=release" alt="Release"></a></p>

<p align="center">
  <a href="https://kmila.com.mx/demo"><b>Prueba la demo en el navegador</b></a> · <a href="https://kmila.com.mx">Sitio web</a> · <a href="https://github.com/Kmila9s/Documentation/releases">Manual de usuario (PDF) y tesis</a>
</p>

## Qué es

La base de tiempo discreta del simulador Kmila: ticks, relojes y eventos de tiempo que usa el motor.

<p align="center"><img src="docs/assets/screenshot.png" alt="" width="90%"></p>

## Inicio rápido

```bash
# TimeMachine se compila junto con sus repos hermanos (referencias de proyecto);
# build.sh clona todos los repos lado a lado en un espacio de trabajo y compila la app
git clone https://github.com/Kmila9s/Kmila.git
Kmila/build.sh --native --workdir ~/kmila-ws
dotnet build ~/kmila-ws/TimeMachine/TimeMachine.csproj
```

## Dónde encaja

```mermaid
flowchart LR
  kmila["Kmila<br/>app · web · CLI"]
  debugger["Debugger"]
  interpreter["Interpreter"]
  parser["Parser"]
  librarycompiler["LibraryCompiler"]
  timemachine["TimeMachine"]
  practicevalidator["PracticeValidator"]
  simulatorbench["SimulatorBench"]
  tests["Tests"]
  ghdl["GHDL 6<br/>(referencia)"]
  kmila-->debugger
  kmila-->interpreter
  kmila-->librarycompiler
  debugger-->interpreter
  interpreter-->parser
  interpreter-->timemachine
  interpreter-->librarycompiler
  librarycompiler-->parser
  practicevalidator-->kmila
  simulatorbench-->kmila
  tests-->kmila
  practicevalidator-.->ghdl
  simulatorbench-.->ghdl
  classDef me fill:#3c82f6,stroke:#1d4ed8,color:#fff
  classDef other fill:#475569,stroke:#334155,color:#fff
  classDef ext fill:#94a3b8,stroke:#64748b,color:#0f172a
  class kmila,debugger,interpreter,parser,librarycompiler,practicevalidator,simulatorbench,tests other
  class ghdl ext
  class timemachine me
  click kmila "https://github.com/Kmila9s/Kmila"
  click debugger "https://github.com/Kmila9s/Debugger"
  click interpreter "https://github.com/Kmila9s/Interpreter"
  click parser "https://github.com/Kmila9s/Parser"
  click librarycompiler "https://github.com/Kmila9s/LibraryCompiler"
  click timemachine "https://github.com/Kmila9s/TimeMachine"
  click practicevalidator "https://github.com/Kmila9s/PracticeValidator"
  click simulatorbench "https://github.com/Kmila9s/SimulatorBench"
  click tests "https://github.com/Kmila9s/Tests"
```

<sub>Las flechas van de un proyecto a lo que usa (referencias de proyecto). El recuadro resaltado es este repositorio.</sub>

## Documentación

- [Referencia técnica (arquitectura, API, historial de cambios)](docs/reference.md)
- [Manual de usuario (PDF) y tesis](https://github.com/Kmila9s/Documentation/releases)

## La familia Kmila

| | | |
|---|---|---|
| <img src="https://github.com/Kmila9s/Kmila/raw/main/docs/assets/icon.png" width="28" alt=""> | [**Kmila**](https://github.com/Kmila9s/Kmila) | Escribe, simula y depura HDL sin FPGA |
| <img src="https://github.com/Kmila9s/Parser/raw/main/docs/assets/icon.png" width="28" alt=""> | [**Parser**](https://github.com/Kmila9s/Parser) | Analizador léxico y sintáctico de VHDL para Kmila |
| <img src="https://github.com/Kmila9s/Interpreter/raw/main/docs/assets/icon.png" width="28" alt=""> | [**Interpreter**](https://github.com/Kmila9s/Interpreter) | Motor de simulación por ciclos delta y síntesis |
| <img src="https://github.com/Kmila9s/Debugger/raw/main/docs/assets/icon.png" width="28" alt=""> | [**Debugger**](https://github.com/Kmila9s/Debugger) | Validación estructural y lint de diseños VHDL |
| <img src="https://github.com/Kmila9s/LibraryCompiler/raw/main/docs/assets/icon.png" width="28" alt=""> | [**LibraryCompiler**](https://github.com/Kmila9s/LibraryCompiler) | Librerías IEEE, cláusulas use y primitivas |
| <img src="https://github.com/Kmila9s/PracticeValidator/raw/main/docs/assets/icon.png" width="28" alt=""> | [**PracticeValidator**](https://github.com/Kmila9s/PracticeValidator) | Valida cada práctica contra GHDL |
| <img src="https://github.com/Kmila9s/SimulatorBench/raw/main/docs/assets/icon.png" width="28" alt=""> | [**SimulatorBench**](https://github.com/Kmila9s/SimulatorBench) | Banco de exactitud Kmila contra GHDL |
| <img src="https://github.com/Kmila9s/Tests/raw/main/docs/assets/icon.png" width="28" alt=""> | [**Tests**](https://github.com/Kmila9s/Tests) | Pruebas unitarias, de extremo a extremo y de rendimiento |
| <img src="https://github.com/Kmila9s/Documentation/raw/main/docs/assets/icon.png" width="28" alt=""> | [**Documentation**](https://github.com/Kmila9s/Documentation/releases) | Tesis y manual de usuario (PDF) |

## Contribuir

Se aceptan issues y merge requests; consulta [CONTRIBUTING.es.md](CONTRIBUTING.es.md).

## Licencia

MIT © 2026 Ulrich Tamayo Daniel. Consulta [LICENSE](LICENSE).

<sub>Desarrollado como Trabajo Terminal TT 2026-B059 en el IPN ESCOM (México).</sub>
