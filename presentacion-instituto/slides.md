---
theme: default
title: Sistema de Gestión Institucional
info: |
  ## Sistema de Gestión Institucional
  Instituto Superior Docente Túpac Amaru — Defensa de Proyecto Final

  Vue 3 + TypeScript + ASP.NET Core 9 + SQL Server
class: text-center
highlighter: shiki
lineNumbers: true
drawings:
  persist: false
transition: slide-left
mdc: true
colorSchema: dark
---

# Sistema de Gestión Institucional

**Instituto Superior Docente Túpac Amaru**

Plataforma de gestión académica — Defensa de Proyecto Final

<div class="pt-12">
  <span class="px-2 py-1 rounded cursor-pointer" hover="bg-white bg-opacity-10">
    Presioná <kbd>espacio</kbd> para navegar →
  </span>
</div>

<Arrow x1="640" y1="475" x2="560" y2="430" color="#888888" width="2" />

<!--
⏱ 30 seg.
Saludo formal antes de arrancar: "Buenas tardes, jurado. Mi nombre es [tu nombre] y voy a presentar mi proyecto final: Sistema de Gestión Institucional, desarrollado para el Instituto Superior Docente Túpac Amaru."
Mencioná el stack en una frase: Vue 3 + ASP.NET Core 9 + SQL Server.
Nota técnica: el <Arrow> apunta al cartel de navegación — si no queda alineado en tu pantalla, ajustá las coordenadas x1/y1/x2/y2 (son solo un ejemplo).
Transición hablada: "Antes de entrar en la arquitectura, quiero contarles por qué construimos este sistema."
-->

---
layout: default
---

# Agenda

1. El problema y la solución
2. Arquitectura del sistema (backend, frontend, base de datos)
3. El viaje de un request, de principio a fin
4. Seguridad: autenticación JWT
5. Reglas de negocio y código real
6. Calidad: testing, métricas y stack
7. Honestidad técnica: qué falta y qué sigue
8. Demo en vivo

<!--
⏱ 30 seg.
Leé la agenda de corrido, sin detenerte en cada punto — el objetivo es dar un mapa mental, no adelantar contenido que vas a explicar después.
Transición: "Arranquemos por el principio: ¿qué problema resuelve este sistema?"
-->

---
layout: quote
transition: fade
---

# ¿Cómo gestiona hoy un instituto sus carreras, sus alumnos y sus formularios?

Planillas sueltas, información dispersa entre oficinas, y ningún lugar único de verdad.

<!--
⏱ 45 seg.
Este slide es retórico, no lo leas literal: usalo como disparador para contar con tus palabras la situación real antes del sistema (inscripciones en papel o Excel, gestión manual, sin trazabilidad entre áreas).
Enfatizar: la falta de un sistema centralizado no es solo incomodidad, es riesgo de error humano y de pérdida de información.
Transición: "Para resolver eso, construimos esto."
-->

---
layout: default
---

# La Solución

Una plataforma web que centraliza la gestión académica del instituto:

<v-clicks>

- **Carreras** — alta, baja, modificación y validación de integridad
- **Alumnos** — inscripción pública + listado para administración
- **Administradores** — gestión autenticada con JWT
- **Profesores** — gestión autenticada (backend listo, frontend en curso)
- **Formularios** — con estados: Borrador / Abierto / Cerrado / Archivado
- **Listados** — vista consolidada de alumnos por carrera
- **Autenticación** — login JWT (8h) + BCrypt (workFactor 12)

</v-clicks>

<!--
⏱ 1.5 min.
Recorré cada módulo con una frase corta — no expliques implementación todavía, eso viene en arquitectura.
Enfatizar que hay DOS públicos distintos: el panel interno (con JWT) y el formulario público de inscripción, sin login.
Transición: "Y todo esto lo usan tres tipos de personas distintas. Veamos quién hace qué."
-->

---
layout: image-right
image: /casos-uso.png
backgroundSize: contain
---

# Roles y Casos de Uso

<v-clicks>

- 👑 **SuperAdmin** — todo lo de Admin + crear otros administradores
- 🔐 **Admin (JWT)** — gestiona carreras, alumnos, profesores, formularios y listados
- 🌐 **Público (sin auth)** — ve carreras, se inscribe como alumno, ve contacto

</v-clicks>

<div class="mt-8 text-sm opacity-70">
Tres niveles de acceso, un mismo sistema.
</div>

<!--
⏱ 1 min.
El jurado suele preguntar por permisos, así que este slide es clave. Remarcá que el endpoint de inscripción es público a propósito: no tiene sentido pedirle login a un alumno que se inscribe por primera vez.
Transición: "Ahora sí, entremos en la arquitectura técnica."
-->

---
layout: default
class: text-center
---

# Arquitectura en Capas

<img src="/simple-3-bloques.png" class="mx-auto mt-4" style="max-height: 210px;" />

<v-click>

<img src="/arquitectura-horizontal.png" class="mx-auto mt-6" style="max-height: 220px;" />

</v-click>

<div class="text-sm mt-4 opacity-70">
N-Tier clásico: Presentación → Negocio → Acceso a Datos → Base de Datos
</div>

<!--
⏱ 1 min.
Arrancá con la vista simple de 3 bloques (Vue ↔ ASP.NET ↔ SQL) para dar la idea general.
Clickeá para abrir el detalle de 5 capas: Vue 3 (presentación) → API (controllers) → BR (reglas de negocio) → AD (acceso a datos) → SQL Server.
Enfatizar: cada capa solo conoce a la de al lado, nunca salta capas — eso es lo que permite testear BR sin tocar la base real.
Transición: "Veamos cada capa del backend en detalle."
-->

---
layout: default
class: text-center
---

# Arquitectura del Backend

<img src="/arquitectura-backend.png" class="mx-auto mt-4" style="max-height: 460px;" />

<!--
⏱ 1 min.
Tres proyectos .NET separados: Instituto.API (controllers + Program.cs con DI/JWT/CORS), Instituto.BR (servicios + interfaces + DTOs ServiceResult), Instituto.AD (repositorios + AccesoDB + modelos).
Enfatizar el patrón ServiceResult: los servicios nunca tiran excepciones de negocio, devuelven un resultado con Success/Message. Las excepciones reales las captura el middleware.
Transición: "Ese era el backend. Ahora el frontend."
-->

---
layout: default
class: text-center
---

# Arquitectura del Frontend

<img src="/arquitectura-frontend.png" class="mx-auto mt-4" style="max-height: 460px;" />

<!--
⏱ 1 min.
19 vistas Vue protegidas por guards de router (meta: requiereAuth / soloInvitado). La autenticación vive en un composable useAuth + sessionStorage. La integración con la API es fetch directo, sin librería externa, devolviendo JSON camelCase.
Si preguntan por Pinia: está configurado, pero el estado principal se maneja con sessionStorage y composables — contestalo con honestidad si te preguntan por qué no se usa más.
Transición: "Con las dos puntas ya presentadas, sigamos los datos: ¿dónde se guardan?"
-->

---
layout: default
class: text-center
---

# Base de Datos — Modelo Entidad-Relación

<img src="/db-er.png" class="mx-auto mt-4" style="max-height: 450px;" />

<!--
⏱ 1 min.
5 tablas: Carreras, Alumnos, Administradores, Profesores, Formularios. Única relación formal por FK: Carreras 1-a-N Alumnos.
Remarcar: Email y DNI tienen restricción UNIQUE — no se puede duplicar un alumno ni un admin por email.
Transición: "Con los datos ya modelados, veamos cómo se representan como clases en C#."
-->

---
layout: default
class: text-center
---

# Jerarquía de Clases (POO)

<img src="/jerarquia-clases.png" class="mx-auto mt-4" style="max-height: 450px;" />

<!--
⏱ 45 seg.
Persona es una clase abstracta (Id, Nombre, Apellido, Email) de la que heredan Alumno, Administrador y Profesor. Evita repetir código y modela bien el dominio: todas las personas del sistema comparten atributos básicos.
Transición: "Toda esta lógica vive organizada en proyectos separados. Veamos la solución completa."
-->

---
layout: default
class: text-center
---

# Estructura del Proyecto

<img src="/estructura-solution.png" class="mx-auto mt-4" style="max-height: 320px;" />

<div class="text-sm mt-4 opacity-70">
Instituto.sln — 5 proyectos: AD, BR, API, Frontend y Tests
</div>

<!--
⏱ 45 seg.
Cada capa es un proyecto .NET independiente con sus propias referencias: API depende de BR, BR depende de AD, pero AD no conoce a nadie de arriba. Esa dirección única de dependencias es lo que hace testeable al sistema.
Transición: "Ya vimos las piezas por separado. Ahora sigamos una request real de punta a punta."
-->

---
layout: default
class: text-center
---

# El Viaje de un Request — 8 Pasos

<img src="/flujo-horizontal.png" class="mx-auto mt-2" style="max-height: 120px;" />

<div class="text-left mt-6 mx-auto" style="max-width: 720px;">
<v-clicks>

1. **Vista** (`CarreraView.vue`) dispara la carga al montarse
2. **Front API** (`useApiFetch.ts`) arma el fetch y agrega el header `Bearer JWT` si hay sesión
3. **HTTP** — `GET /api/carreras` viaja por la red
4. **Controller** (`CarreraController`) recibe la request, marcada `[AllowAnonymous]`
5. **Service** (`CarreraService`) aplica las reglas de negocio
6. **Repository** (`CarreraRepository`) arma la consulta
7. **AccesoDB** ejecuta el `SELECT` parametrizado contra SQL Server
8. La respuesta vuelve por el mismo camino, transformándose en cada capa

</v-clicks>
</div>

<!--
⏱ 1.5 min.
Probablemente el slide más importante de la arquitectura: muestra que entendés el recorrido completo, no solo capas sueltas sino cómo hilan entre sí.
Ir clickeando paso a paso, señalando la imagen de arriba en cada uno.
Enfatizar el punto 4: aunque el usuario esté logueado y mande el JWT, el endpoint de listar carreras es público — Admin y visitante ven exactamente lo mismo acá.
Transición: "Veamos ahora el mismo recorrido con el diagrama de secuencia completo."
-->

---
layout: full
class: text-center
transition: slide-up
---

# Diagrama de Secuencia Completo

<img src="/sequence-request.png" class="mx-auto" style="max-height: 560px;" />

<!--
⏱ 1 min.
Mismo recorrido que el slide anterior, pero acá se ve explícito el camino de ida (banda superior) y el camino de vuelta (banda inferior), con los 12 pasos numerados.
Si preguntan "¿por qué separar en capas si total es lo mismo?": cada capa transforma el dato (SqlDataReader → List&lt;Carrera&gt; → ServiceResult → ApiResponse camelCase), y eso permite testear y cambiar una capa sin romper las demás.
Transición: "Listar carreras es público. Pero el login sí necesita seguridad de verdad — veamos cómo funciona."
-->

---
layout: full
class: text-center
---

# Flujo de Autenticación

<img src="/auth-jwt.png" class="mx-auto" style="max-height: 560px;" />

<!--
⏱ 1.25 min.
Recorré los 15 pasos sin leerlos uno por uno: el usuario manda email+password, el AuthController delega al AdministradorService, que busca por email y compara el hash con BCrypt.Verify. Si es válido, se genera un JWT de 8 horas con los claims (sub, email, role).
Enfatizar el bloque amarillo de abajo: a partir de ahí, cada request siguiente manda el token en el header Authorization vía el composable authHeaders().
Transición: "Ahora, por qué esto es seguro y no solo funcional."
-->

---
layout: default
---

# Seguridad en Profundidad

<v-clicks>

- **BCrypt, workFactor 12** — hash adaptativo, pensado para ser lento a propósito (dificulta ataques de fuerza bruta)
- **JWT firmado, 8 horas de expiración** — claims mínimos: `sub`, `email`, `role`
- **SQL parametrizado en toda la capa AD** — cero concatenación de strings, cero SQL injection
- **Nada de contraseñas en texto plano**, ni en la base ni en los logs
- **Pendiente para v2.0:** refresh tokens y 2FA (lo vemos en el roadmap)

</v-clicks>

<!--
⏱ 45 seg.
Slide conceptual, no repitas el diagrama anterior. La idea es mostrar que las decisiones de seguridad fueron conscientes, no casualidad.
Si preguntan "¿por qué 8 horas y no más o menos?": es un balance entre comodidad de uso (no relogueás todo el tiempo) y ventana de riesgo si roban el token.
Transición: "La seguridad de acceso es una cosa. El estado de los datos es otra — veamos el ciclo de vida de un formulario."
-->

---
layout: default
class: text-center
---

# Estados de Formularios

<img src="/formulario-estados.png" class="mx-auto mt-4" style="max-height: 460px;" />

<!--
⏱ 45 seg.
Cuatro estados: Borrador (editable, no público) → Abierto (visible, acepta inscripciones) → Cerrado (solo lectura) → Archivado (fin del ciclo). Cerrado también puede volver a Borrador con Reabrir.
Es un ejemplo de regla de negocio que vive en la capa BR, no en la base de datos ni en el frontend.
Transición: "Hablando de reglas de negocio, veamos cómo se ve el código real detrás de todo esto."
-->

---
layout: default
---

# Código Real — `CarreraController`

Ejemplo representativo del patrón que se repite en los 8 controllers:

<Transform :scale="1.02" origin="top center">

```csharp {1-6|8-18|20-30|all}
[ApiController]
[Route("api/[controller]")]
public class CarreraController : ControllerBase
{
    private readonly ICarreraService _service;
    public CarreraController(ICarreraService service) => _service = service;

    [HttpGet]
    [AllowAnonymous]
    public IActionResult GetAll()
    {
        ServiceResult<List<Carrera>> result = _service.GetAll();

        if (!result.Success)
            return StatusCode(500, new { error = result.Message });

        return Ok(result.Data);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public IActionResult Create([FromBody] Carrera carrera)
    {
        ServiceResult<Carrera> result = _service.Create(carrera);

        if (!result.Success)
            return BadRequest(new { error = result.Message });

        return CreatedAtAction(nameof(GetAll), result.Data);
    }
}
```

</Transform>

<div class="text-sm mt-2 opacity-60">
⚠️ Reemplazá este bloque por tu <code>CarreraController.cs</code> real antes de presentar.
</div>

<!--
⏱ 1.25 min.
IMPORTANTE: este código es ilustrativo, armado a partir de los patrones que se ven en tus propios diagramas (ServiceResult, AllowAnonymous en el GET, Authorize por rol en el POST). Reemplazalo por tu controller real copiado y pegado — el jurado puede pedirte que abras el archivo real en el editor, así que esto tiene que ser exactamente tu código.
Al presentar: el primer click resalta el constructor con inyección de dependencias, el segundo el GET público, el tercero el POST protegido por rol.
Remarcá el patrón: el controller nunca decide si algo es válido, solo traduce el resultado del service a un código HTTP.
Transición: "Este patrón se repite en los 8 controllers. Pero un patrón solo vale si está probado — hablemos de testing."
-->

---
layout: default
---

# Manejo de Errores

| Caso | Origen | Captura | HTTP |
|------|--------|---------|------|
| Validación de negocio | Service | Controller | **400 / 404** |
| Excepción no controlada | Cualquier capa | `UseExceptionHandler` | **500** |
| JWT inválido o expirado | Middleware | `[Authorize]` | **401** |
| Error de base de datos | AD | Controller | **500** |

<v-click>

<div class="mt-8 text-center text-lg">
✅ Ningún stack trace llega al cliente — siempre <code>{ "error": "..." }</code>
</div>

</v-click>

<!--
⏱ 45 seg.
Remarcá el principio general antes que la tabla: todo error se traduce a un JSON consistente, nunca se filtra información interna (nombres de tablas, stack traces, rutas de archivos).
Para revisar antes de la defensa: el diagrama de secuencia muestra el header Bearer JWT viajando incluso en /api/carreras, que está marcado [AllowAnonymous]. No es necesariamente un error (el frontend puede adjuntar el token siempre que exista), pero convendría confirmarlo en tu código real por si el jurado pregunta.
Transición: "Hablando de robustez, veamos qué tan cubierto está el sistema con tests."
-->

---
layout: fact
---

# 44

Tests automatizados, distribuidos en las tres capas del backend

<!--
⏱ 15 seg.
Un respiro visual antes de entrar en el detalle. Dejalo un segundo en pantalla, no lo expliques todavía.
Transición: "¿Cómo se reparten esos 44 tests?"
-->

---
layout: default
---

# Testing — Desglose por Capa

<div class="grid grid-cols-3 gap-4 mt-8">

<v-click>
<div class="rounded-lg p-4 bg-blue-700 bg-opacity-80">
  <div class="text-4xl font-bold text-center">20</div>
  <div class="text-center mt-2">Instituto.AD</div>
  <div class="text-xs text-center mt-1 opacity-80">Acceso a datos</div>
</div>
</v-click>

<v-click>
<div class="rounded-lg p-4 bg-orange-600 bg-opacity-80">
  <div class="text-4xl font-bold text-center">17</div>
  <div class="text-center mt-2">Instituto.BR</div>
  <div class="text-xs text-center mt-1 opacity-80">Reglas de negocio (Moq)</div>
</div>
</v-click>

<v-click>
<div class="rounded-lg p-4 bg-purple-700 bg-opacity-80">
  <div class="text-4xl font-bold text-center">7</div>
  <div class="text-center mt-2">Instituto.API</div>
  <div class="text-xs text-center mt-1 opacity-80">Contratos HTTP</div>
</div>
</v-click>

</div>

<div class="mt-8 text-sm opacity-70">
MSTest + Moq — cada capa se testea en aislamiento, mockeando la capa de abajo.
</div>

<!--
⏱ 1 min.
Explicá por qué la proporción tiene sentido: la mayoría de los tests están en AD (20) porque ahí vive el riesgo real de bugs contra SQL Server; BR (17) valida reglas de negocio mockeando el repositorio; API (7) es la capa más fina, solo valida que el controller devuelva el código HTTP correcto.
Si preguntan "¿por qué no hay más tests de API?": porque la lógica pesada ya está cubierta en las capas de abajo, el controller es casi un traductor.
Transición: "Con el código probado, veamos los números generales del proyecto."
-->

---
layout: default
class: text-center
---

# Métricas del Proyecto

<img src="/metricas.png" class="mx-auto mt-4" style="max-height: 430px;" />

<!--
⏱ 30 seg.
Slide rápido, casi de transición: 8 controllers, 6 servicios, 6 repositorios, 44 tests, 19 vistas, 5 tablas. No hace falta reexplicar cada número, ya se explicaron por separado.
Transición: "Para cerrar la parte técnica, el stack completo."
-->

---
layout: two-cols
---

# Stack Tecnológico

### Backend

- ASP.NET Core 9
- ADO.NET (`Microsoft.Data.SqlClient`)
- JWT Bearer + BCrypt 12
- MSTest + Moq (44 tests)

::right::

<div class="mt-16">

### Frontend

- Vue 3.5 + TypeScript 5.8
- Vite 7 + Vue Router 4
- Pinia + Element Plus
- CSS modular

</div>

<!--
⏱ 30 seg.
Sin ORM de por medio: ADO.NET puro con SQL parametrizado, decisión consciente para entender bien qué pasa en cada consulta.
Transición: "Ahora la parte más honesta de la charla: de dónde veníamos y qué nos falta."
-->

---
layout: two-cols-header
---

# Antes / Después — De JSON a SQL Server

::left::

### 🟡 v1.0 — Antes

- Persistencia en archivos **JSON planos**
- Sin integridad relacional
- Riesgo de corrupción con escrituras simultáneas
- Consultas armadas a mano recorriendo listas

::right::

### 🟢 v1.1 — Ahora

- **SQL Server** vía ADO.NET parametrizado
- Foreign Keys y constraints (`UNIQUE`, `PK`, `FK`)
- Transacciones y locks del motor de base de datos
- `SELECT` con joins e índices reales

<!--
⏱ 45 seg.
Contá esto como una decisión de madurez, no como un error corregido: al principio priorizamos tener algo funcionando rápido, y cuando el dominio creció (relaciones entre carreras y alumnos, validaciones de unicidad) migramos a un motor relacional de verdad.
Transición: "Y hablando de decisiones honestas, veamos qué sabemos que todavía falta."
-->

---
layout: default
---

# Deuda Técnica Conocida

<div class="flex flex-col gap-4 mt-4">

<v-click>
<div class="border-l-4 border-red-500 pl-4">
  <div class="font-bold">🔴 Crítica</div>
  <div class="text-sm mt-1">Bug <code>CarreraId: string</code> vs <code>number</code> en <code>InscripciónView.vue</code> · Frontend de Profesores sin vistas (backend ya listo)</div>
</div>
</v-click>

<v-click>
<div class="border-l-4 border-orange-500 pl-4">
  <div class="font-bold">🟠 Alta</div>
  <div class="text-sm mt-1">Capa <code>src/services/</code> no centralizada · El GET de admins expone <code>PasswordHash</code>/<code>Role</code> · Sin rate limiting</div>
</div>
</v-click>

<v-click>
<div class="border-l-4 border-yellow-500 pl-4">
  <div class="font-bold">🟡 Media</div>
  <div class="text-sm mt-1">Sin paginación en listados · Sin Swagger/OpenAPI</div>
</div>
</v-click>

</div>

<div class="mt-6 text-center text-sm opacity-70">
No la escondemos: sabemos qué falta y por qué todavía no está.
</div>

<!--
⏱ 1 min.
Este slide genera respeto si se presenta con seguridad, no con vergüenza. Frase clave: "esto no es lo que no supimos hacer, es lo que priorizamos no hacer todavía".
Preparate para que te pregunten puntualmente por el bug de CarreraId — tené lista la explicación técnica (probablemente un mismatch de tipo entre el modelo del backend y el formulario del frontend).
Transición: "Y lo que sigue, ya lo tenemos planificado."
-->

---
layout: default
class: text-center
---

# Roadmap del Proyecto

<img src="/roadmap-timeline.png" class="mx-auto mt-4" style="max-height: 300px;" />

<!--
⏱ 45 seg.
v1.0 y v1.1 ya cerradas. v1.2 (próximo) ataca justo la deuda crítica recién mostrada: fix del bug de CarreraId y vistas de Profesores. v2.0 es la visión a mediano plazo: rate limiting, Swagger, paginación, 2FA y refresh tokens.
Transición: "Ya vimos todo en diagramas. Ahora se lo muestro andando de verdad."
-->

---
layout: default
---

# Demo en Vivo

<v-clicks>

1. Vista pública — abrir `/inscripcion` y completar el formulario de un alumno
2. Login — entrar como Admin en `/login`
3. `/carreras` — crear una carrera nueva y mostrar la validación en acción
4. `/formularios` — cambiar el estado de Borrador a Abierto
5. Cerrar sesión y mostrar que `/carreras` (modo admin) ya no es accesible

</v-clicks>

<div class="mt-8 p-3 rounded text-sm" style="background-color: rgba(255,255,255,0.08);">
🛟 <b>Plan B:</b> si falla el entorno en vivo, tengo capturas de pantalla o un video de respaldo de cada paso.
</div>

<!--
⏱ 2.5 min de demo real (no de lectura de la slide).
Practicá este recorrido antes de la defensa, al menos dos veces, con la base de datos en un estado conocido. Si algo falla en vivo, no te pongas nervioso: tenés el plan B.
No leas los pasos en voz alta uno por uno: hacé la acción y narrá qué está pasando por detrás (qué controller, qué service se está ejecutando).
Transición: "Con el sistema funcionando en vivo, cierro la presentación."
-->

---
layout: center
class: text-center
transition: fade
---

<div class="text-6xl font-bold mb-4">🏛️</div>

# Sistema de Gestión Institucional

**Instituto Superior Docente Túpac Amaru**

<div class="mt-6 opacity-80">
N-Tier bien separado, JWT + BCrypt, 44 tests, y una deuda técnica que conocemos y priorizamos.
</div>

<div class="mt-10 text-sm opacity-60">
Vue 3 + TypeScript + Vite · ASP.NET Core 9 · SQL Server
</div>

<div class="mt-10">
Gracias por su tiempo — quedo a disposición para preguntas.
</div>

<!--
⏱ 45 seg.
Cerrá reafirmando en una frase qué construiste y por qué las decisiones técnicas fueron conscientes, no casualidad. Agradecé al jurado y a quien corresponda (tutor, instituto) antes de abrir preguntas.
Para las preguntas, tené a mano: el código real de CarreraController.cs, el diagrama de base de datos, y la carpeta Docs/ si existe.
No hay más slides después de esta en el recorrido principal — las dos siguientes son de respaldo, solo mostralas si te preguntan puntualmente por algo que cubren.
-->

---
layout: full
class: text-center
hideInToc: true
---

# Extra — Transformación de Datos en el Request

<img src="/flujo-vertical.png" class="mx-auto" style="max-height: 550px;" />

<!--
Solo mostrar si preguntan específicamente cómo cambian los tipos de dato entre capas.
Vista vertical: SqlDataReader crudo → List<Carrera> en PascalCase → ServiceResult → ApiResponse en camelCase para el frontend.
No forma parte del recorrido principal — es material de respaldo para preguntas puntuales.
-->

---
layout: default
hideInToc: true
---

# Extra — Índice Completo

<Toc columns="2" />

<!--
Slide de respaldo generado automáticamente a partir de los títulos de cada slide (componente Toc de Slidev). Útil como mapa de navegación rápida si el jurado pide volver a un tema puntual.
-->