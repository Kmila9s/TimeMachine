[English](CONTRIBUTING.md)

# Contribuir a TimeMachine

Gracias por tu interés en Kmila.

- **Errores:** abre un issue con el VHDL que lo reproduce (lo más pequeño posible), lo que esperabas y lo que hizo Kmila. Si GHDL 6 se comporta distinto, incluye su salida: Kmila se verifica contra él.
- **Cambios:** abre un merge/pull request contra `main`. Un tema por solicitud y con una prueba.
- **Principio:** Kmila se adapta al código, nunca el código a Kmila. Las pruebas simulan la fuente original tal cual; los diseños no se reescriben para que el simulador los acepte.
- **Compilación:** .NET 10. Los repos del motor se compilan juntos; `Kmila/build.sh` los clona lado a lado.

El repositorio principal vive en un GitLab privado y se refleja aquí; los cambios aceptados se aplican allá y aparecen en este espejo.

Al contribuir aceptas que tu contribución se publique bajo la Licencia MIT.
