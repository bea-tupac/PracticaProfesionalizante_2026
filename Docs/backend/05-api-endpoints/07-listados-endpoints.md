# Endpoints de Listados (Reportes)

**Base Path**: `/api/listado`  
**Controller**: `ListadoController`  
**Autenticación**: **Requerida** (`[Authorize]`)

---

## GET /api/listado

Genera listado consolidado de alumnos con nombre de carrera (join en memoria).

### Request
```http
GET /api/listado
Authorization: Bearer <token>
```

### Response 200 OK
```json
[
  {
    "alumnoId": 1,
    "nombreCompleto": "Pérez, Juan",
    "dni": 12345678,
    "email": "juan@email.com",
    "carrera": "Tecnicatura en Programación",
    "turno": "Mañana",
    "edad": 26
  },
  {
    "alumnoId": 2,
    "nombreCompleto": "Gómez, María",
    "dni": 87654321,
    "email": "maria@email.com",
    "carrera": "Tecnicatura en Diseño Gráfico",
    "turno": "Tarde",
    "edad": 22
  },
  {
    "alumnoId": 3,
    "nombreCompleto": "López, Carlos",
    "dni": 11223344,
    "email": "carlos@email.com",
    "carrera": "Sin carrera",
    "turno": "Noche",
    "edad": 24
  }
]
```

### DTO: AlumnoListadoDto

```csharp
public class AlumnoListadoDto
{
    public int AlumnoId { get; set; }
    public string? NombreCompleto { get; set; }  // "Apellido, Nombre"
    public int DNI { get; set; }
    public string? Email { get; set; }
    public string? Carrera { get; set; }         // Nombre carrera o "Sin carrera"
    public string? Turno { get; set; }
    public int Edad { get; set; }
}
```

---

## Implementación (Join en Memoria)

```csharp
[HttpGet]
public IActionResult GetListado()
{
    var alumnos = _alumnoService.GetAll();      // SELECT * FROM Alumnos
    var carreras = _carreraService.GetAll();    // SELECT * FROM Carreras

    var listado = alumnos.Select(a =>
    {
        var carrera = carreras.FirstOrDefault(c => c.Id == a.CarreraId);
        return new AlumnoListadoDto
        {
            AlumnoId = a.Id,
            NombreCompleto = $"{a.Apellido}, {a.Nombre}",
            DNI = a.DNI,
            Email = a.Email,
            Carrera = carrera?.Nombre ?? "Sin carrera",
            Turno = a.Turno,
            Edad = a.Edad
        };
    }).ToList();

    return Ok(listado);
}
```

**Nota**: Join en memoria (C# LINQ) no en SQL. Para pocos miles de registros es aceptable. Para datasets grandes → mover a SQL con JOIN.

---

## Casos de Uso

| Caso | Descripción |
|------|-------------|
| **Grilla admin** | Vista tabular alumnos + carrera para gestión |
| **Exportar Excel** | Frontend consume y exporta |
| **Reportes** | Base para reportes de inscripción por carrera |

---

## Filtros Futuros (No Implementados)

```
GET /api/listado?carreraId=1&turno=Mañana&search=perez&page=1&pageSize=50
```

| Filtro | Tipo | Descripción |
|--------|------|-------------|
| carreraId | int | Filtrar por carrera |
| turno | string | Filtrar por turno alumno |
| search | string | Buscar en nombre/apellido/email/DNI |
| page/pageSize | int | Paginación |

---

## Performance

| Métrica | Actual | Objetivo |
|---------|--------|----------|
| Alumnos | ~500 | < 100ms |
| Carreras | ~20 | < 10ms |
| Join memoria | O(n×m) | O(n+m) con HashSet |
| Total | ~50ms | < 100ms |

**Optimización futura**:
```csharp
// Usar Dictionary para lookup O(1)
var carreraDict = carreras.ToDictionary(c => c.Id);
var listado = alumnos.Select(a => new AlumnoListadoDto
{
    Carrera = carreraDict.TryGetValue(a.CarreraId, out var c) ? c.Nombre : "Sin carrera",
    // ...
}).ToList();
```