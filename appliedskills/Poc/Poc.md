# Tema: Implementación de un modelo de Azure OpenAI para la aplicación de prueba de concepto (POC)

**De:** projman@contoso.com
**Para:** User1-51831849@cloudslice.onmicrosoft.com

Hola:

Vamos a crear una aplicación de prueba de concepto (POC) para Azure OpenAI. La aplicación se almacena en `C:\PoC`. El Azure OpenAI Service ya se ha implementado en el grupo de recursos `RG1lod51831849`. Queremos implementar un modelo y modificar la aplicación de consola para usar el modelo.
Esta es la tarea:

Es necesario implementar un modelo de Azure OpenAI `gpt-35-turbo` denominado **gpt35**. Asegúrate de que están configuradas las siguientes opciones avanzadas:

* Límite de frecuencia de tokens por minuto (miles): *5k*
* Habilitación de la cuota dinámica: *desactivada*
* Después, debes modificar la aplicación POC para que cumpla los siguientes requisitos:

    * Versión de referencia `1.37.0` del paquete NuGet Microsoft.SemanticKernel.
    * Crea un objeto kernel.
    * Usa el modelo **gpt35** que has implementado.
    * Recopila la entrada del usuario y muestra la respuesta del modelo.

Se han agregado los siguientes comentarios al código de la aplicación para indicar dónde deben realizarse los cambios:

- TODO 1.1
- TODO 1.2
- TODO 1.3
- TODO 1.4
- TODO 1.5

Modifica solo el código de estas secciones.

Gracias,

Equipo de administración de proyectos