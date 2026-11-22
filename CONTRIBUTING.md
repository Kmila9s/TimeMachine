[Español](CONTRIBUTING.es.md)

# Contributing to TimeMachine

Thanks for your interest in Kmila.

- **Bugs:** open an issue with the VHDL that reproduces it (as small as possible), what you expected and what Kmila did. If GHDL 6 behaves differently, include its output: Kmila is checked against it.
- **Changes:** open a merge/pull request against `main`. Keep one topic per request and include a test.
- **Principle:** Kmila adapts to the code, never the code to Kmila. Tests simulate the original source verbatim; designs are not rewritten so the simulator accepts them.
- **Build:** .NET 10. The engine repos are built together; `Kmila/build.sh` clones them side by side.

The primary repository lives on a private GitLab instance and is mirrored here, so accepted changes are applied there and appear in this mirror.

By contributing you agree that your contribution is licensed under the MIT License.
