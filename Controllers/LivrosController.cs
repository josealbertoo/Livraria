using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using System.Net.NetworkInformation;
using System.Text.Json;

namespace Livraria.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class LivrosController : ControllerBase
    {
        public static T GetEnumValue<T>(string str) where T : struct, IConvertible
        {
            Type enumType = typeof(T);
            if (!enumType.IsEnum)
            {
                throw new Exception("T must be an Enumeration type.");
            }
            return Enum.TryParse(str, true, out T val) ? val : default;
        }

        public static T GetEnumValue<T>(int intValue) where T : struct, IConvertible
        {
            Type enumType = typeof(T);
            if (!enumType.IsEnum)
            {
                throw new Exception("T must be an Enumeration type.");
            }

            return (T)Enum.ToObject(enumType, intValue);
        }

        [HttpGet()] //Name = "GetLivros"
        [ProducesResponseType(typeof(List<Livros>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(String), StatusCodes.Status400BadRequest)]
        public IActionResult GetLivros()
        {
            CriarTabelaSQlite();
            var (livros, errou) = GetClientes();
            return (livros != null) ? Ok(livros) : ((errou != null) ? NotFound(errou) : NotFound("falhou"));
        }

        [HttpGet()]
        [Route("{id}")]
        [ProducesResponseType(typeof(Livros), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(String), StatusCodes.Status400BadRequest)]
        public IActionResult Getbyid([FromRoute] int id)
        {

            CriarTabelaSQlite();
            var (livros, errou) = GetCliente(id);

            return (livros != null) ? Ok(livros) : ((errou != null) ? NotFound(errou) : NotFound("falhou"));

        }

        [HttpPost()]
        [ProducesResponseType(typeof(Livros), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(List<Livros>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(String), StatusCodes.Status400BadRequest)]
        public IActionResult PostLivro([FromBody] Livros livro)
        {

            CriarTabelaSQlite();

            var (rLivro,rLivros, errou) = Add(livro);

            return (rLivros != null) ? Ok(rLivros) : (rLivro != null) ? Ok(rLivro) : ((errou != null) ? NotFound(errou) : NotFound("falhou"));

        }

        [HttpPost("Lista")]
        [ProducesResponseType(typeof(Livros), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(List<Livros>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(String), StatusCodes.Status400BadRequest)]
        public IActionResult PostLivros([FromBody] List<Livros> livros)
        {
            
            CriarTabelaSQlite();

            var (rLivro, rLivros, errou) = Add(livros);

            return (rLivros != null) ? Ok(rLivros) : (rLivro != null) ? Ok(rLivro) : ((errou != null) ? NotFound(errou) : NotFound("falhou"));

        }
        

        [HttpPut()]
        [ProducesResponseType(typeof(Livros), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(String), StatusCodes.Status400BadRequest)]
        public IActionResult PutLivro([FromBody] Livros livro)
        {
            if (livro.Id == 0)
                return NotFound("Id Invalido ou não informado.");

            CriarTabelaSQlite();
            
            var (rLivro, rLivros, errou) = Update(livro);

            return (rLivros != null) ? Ok(rLivros) : (rLivro != null) ? Ok(rLivro) : ((errou != null) ? NotFound(errou) : NotFound("falhou"));

        }

        [HttpPut()]
        [Route("{id}")]
        [ProducesResponseType(typeof(Livros), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(String), StatusCodes.Status400BadRequest)]
        public IActionResult PutLivro([FromRoute] int id, [FromBody] Livros livro)
        {
            if (id != livro.Id)
                if (livro.Id != 0) // se livros.Id for 0 pode ser que a pessoa nao tenha colocado no json o id mais aí nao tem problema pq pega id da rota...
                    return NotFound("id Informado na rota é diferente do id informado no body.");
                else
                    livro.Id = id;
            if (id == 0)
                return NotFound("Id Invalido.");

            CriarTabelaSQlite();

            var (rLivro, rLivros, errou) = Update(livro);

            return (rLivros != null) ? Ok(rLivros) : (rLivro != null) ? Ok(rLivro) : ((errou != null) ? NotFound(errou) : NotFound("falhou"));

        }

        [HttpPut("Lista")]
        [ProducesResponseType(typeof(Livros), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(List<Livros>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(String), StatusCodes.Status400BadRequest)]
        public IActionResult PutLivros([FromBody] List<Livros> livros)
        {
            
            CriarTabelaSQlite();

            var (rLivro, rLivros, errou) = Update(livros);

            return (rLivros != null) ? Ok(rLivros) : (rLivro != null) ? Ok(rLivro) : ((errou != null) ? NotFound(errou) : NotFound("falhou"));

        }

        private static SqliteConnection DbConnection()
        {
            var builder = WebApplication.CreateBuilder();
            var DbPath = builder.Configuration["ConnectionStrings:Lightdbdatabase"];
            using var conn = new SqliteConnection($"Data Source={DbPath}");
            conn.Open();
            return conn;
        }

        public static void CriarTabelaSQlite()
        {
            try
            {
                using var conn = DbConnection();
                if (conn.State == System.Data.ConnectionState.Closed)
                    conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText = "CREATE TABLE IF NOT EXISTS Livros(id INTEGER PRIMARY KEY AUTOINCREMENT, cTitulo Varchar(80) DEFAULT '                                                                                ' NOT NULL, cAutor VarChar(80) DEFAULT '                                                                                ' NOT NULL,cGenero Varchar(30) DEFAULT '                              ' NOT NULL,nPreco REAL DEFAULT 0 NOT NULL,nQuantidade REAL DEFAULT 0 NOT NULL,Date Datetime DEFAULT current_timestamp)";
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Exception ex1 = ex;
                throw ex1;
            }
        }

        public static (List<Livros>?, string?) GetClientes()
        {
            var livros = new List<Livros>();
            try
            {
                using (var conn = DbConnection())
                {
                    if (conn.State == System.Data.ConnectionState.Closed)
                        conn.Open();
                    var cmd = conn.CreateCommand();
                    cmd.CommandText = "SELECT * FROM Livros ";

                    using var reader = cmd.ExecuteReader();
                    do
                    {
                        while (reader.Read())
                        {
                            var livro = new Livros(DateOnly.FromDateTime(DateTime.Now));

                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                switch (reader.GetName(i))
                                {
                                    case "cTitulo":
                                        livro.Titulo = reader.GetFieldValue<string>(i);
                                        break;
                                    case "cAutor":
                                        livro.Autor = reader.GetFieldValue<string>(i);
                                        break;
                                    case "cGenero":
                                        livro.Genero = GetEnumValue<Generos>(reader.GetFieldValue<string>(i));
                                        break;
                                    case "nPreco":
                                        livro.Preco = reader.GetFieldValue<double>(i);
                                        break;
                                    case "nQuantidade":
                                        livro.Quantidade = reader.GetFieldValue<double>(i);
                                        break;
                                    case "Date":
                                        livro.Date = reader.GetFieldValue<DateOnly>(i);
                                        break;
                                    case "id":
                                        livro.Id = reader.GetFieldValue<int>(i);
                                        break;
                                }
                            }

                            livros.Add(livro);

                        }
                    } while (reader.NextResult());
                }
                return (livros,null);
            }
            catch (Exception ex)
            {
                Exception ex1 = ex;
                throw ex1;
            }
        }

        public static (Livros?, string?) GetCliente(int id)
        {
            try
            {
                using var conn = DbConnection();
                if (conn.State == System.Data.ConnectionState.Closed)
                    conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT * FROM Livros Where id = @id ";
                cmd.Parameters.AddWithValue("@id", id);

                using var reader = cmd.ExecuteReader();
                do
                {
                    while (reader.Read())
                    {

                        var livro = new Livros(DateOnly.FromDateTime(DateTime.Now));


                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            switch (reader.GetName(i))
                            {
                                case "cTitulo":
                                    livro.Titulo = reader.GetFieldValue<string>(i);
                                    break;
                                case "cAutor":
                                    livro.Autor = reader.GetFieldValue<string>(i);
                                    break;
                                case "cGenero":
                                    livro.Genero = GetEnumValue<Generos>(reader.GetFieldValue<string>(i));
                                    break;
                                case "nPreco":
                                    livro.Preco = reader.GetFieldValue<double>(i);
                                    break;
                                case "nQuantidade":
                                    livro.Quantidade = reader.GetFieldValue<double>(i);
                                    break;
                                case "Date":
                                    livro.Date = reader.GetFieldValue<DateOnly>(i);
                                    break;
                                case "id":
                                    livro.Id = reader.GetFieldValue<int>(i);
                                    break;
                            }
                        }

                        if (conn.State != System.Data.ConnectionState.Closed)
                            conn.Close();
                        return (livro, null);
                    }
                } while (reader.NextResult());
            }
            catch (Exception ex)
            {
                Exception ex1 = ex;
                throw ex1;

            }
            return (null, " Nao encontrado");
        }
        public static (Livros?, List<Livros>?, string?) Add(object lista)
        {
            try
            {
                List<Livros> rlivros = [];
                if (lista.GetType() == typeof(Livros))
                {

                    using var conn = DbConnection();
                    if (lista is Livros livro)
                    {
                        if (conn.State == System.Data.ConnectionState.Closed)
                            conn.Open();
                        var cmd = conn.CreateCommand();
                        cmd.CommandText = "INSERT INTO Livros(cTitulo, cAutor, cGenero, nPreco, nQuantidade, Date ) values (@cTitulo, @cAutor, @cGenero, @nPreco, @nQuantidade, @Date) RETURNING *";
                        cmd.Parameters.AddWithValue("@cTitulo", livro.Titulo);
                        cmd.Parameters.AddWithValue("@cAutor", livro.Autor);
                        cmd.Parameters.AddWithValue("@cGenero", livro.Genero.ToString());
                        cmd.Parameters.AddWithValue("@nPreco", livro.Preco);
                        cmd.Parameters.AddWithValue("@nQuantidade", livro.Quantidade);
                        cmd.Parameters.AddWithValue("@Date", livro.Date);
                        //cmd.ExecuteNonQuery();
                        //SqliteDataReader sqliteDataReader = ExecuteReader();
                        //sqliteDataReader.Dispose();

                        using var reader = cmd.ExecuteReader();
                        do
                        {
                            while (reader.Read())
                            {

                                var rlivro = new Livros(DateOnly.FromDateTime(DateTime.Now));


                                for (int i = 0; i < reader.FieldCount; i++)
                                {
                                    switch (reader.GetName(i))
                                    {
                                        case "cTitulo":
                                            rlivro.Titulo = reader.GetFieldValue<string>(i);
                                            break;
                                        case "cAutor":
                                            rlivro.Autor = reader.GetFieldValue<string>(i);
                                            break;
                                        case "cGenero":
                                            rlivro.Genero = GetEnumValue<Generos>(reader.GetFieldValue<string>(i));
                                            break;
                                        case "nPreco":
                                            rlivro.Preco = reader.GetFieldValue<double>(i);
                                            break;
                                        case "nQuantidade":
                                            rlivro.Quantidade = reader.GetFieldValue<double>(i);
                                            break;
                                        case "Date":
                                            rlivro.Date = reader.GetFieldValue<DateOnly>(i);
                                            break;
                                        case "id":
                                            rlivro.Id = reader.GetFieldValue<int>(i);
                                            break;
                                    }
                                }

                                if (conn.State != System.Data.ConnectionState.Closed)
                                    conn.Close();
                                return (rlivro, null, null);

                            }
                        } while (reader.NextResult());
                    }
                    else
                        return (null, null, " Livro nullo");

                }
                else if (lista.GetType() == typeof(List<Livros>))
                {
                    if (lista is not List<Livros> livros)
                        return (null, null, " Lista de Livros vazia");
                    if (livros.Count == 0)
                        return (null, null, " Lista de Livros vazia");
                    using var conn = DbConnection();

                    if (conn.State == System.Data.ConnectionState.Closed)
                        conn.Open();
                    var cmd = conn.CreateCommand();
                    cmd.CommandText = "INSERT INTO Livros(cTitulo, cAutor, cGenero, nPreco, nQuantidade, Date ) values ";
                    var ncontador = 0;
                    foreach (Livros livro in livros)
                    {
                        ncontador++;
                        cmd.CommandText += "(";
                        cmd.CommandText += "  @cTitulo" + ncontador.ToString() + ",";
                        cmd.CommandText += "  @cAutor" + ncontador.ToString() + ",";
                        cmd.CommandText += "  @cGenero" + ncontador.ToString() + ",";
                        cmd.CommandText += "  @nPreco" + ncontador.ToString() + ",";
                        cmd.CommandText += "  @nQuantidade" + ncontador.ToString() + ",";
                        cmd.CommandText += "  @Date" + ncontador.ToString();
                        cmd.CommandText += ")";
                        if (livros.Count == ncontador)
                        {
                            cmd.CommandText += " RETURNING *";
                        }
                        else
                        {
                            cmd.CommandText += ",";
                        }
                        cmd.Parameters.AddWithValue("@cTitulo" + ncontador.ToString(), livro.Titulo);
                        cmd.Parameters.AddWithValue("@cAutor" + ncontador.ToString(), livro.Autor);
                        cmd.Parameters.AddWithValue("@cGenero" + ncontador.ToString(), livro.Genero.ToString());
                        cmd.Parameters.AddWithValue("@nPreco" + ncontador.ToString(), livro.Preco);
                        cmd.Parameters.AddWithValue("@nQuantidade" + ncontador.ToString(), livro.Quantidade);
                        cmd.Parameters.AddWithValue("@Date" + ncontador.ToString(), livro.Date);
                    }
                    //cmd.ExecuteNonQuery();
                    using var reader = cmd.ExecuteReader();
                    do
                    {
                        while (reader.Read())
                        {

                            var rlivro = new Livros(DateOnly.FromDateTime(DateTime.Now));


                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                switch (reader.GetName(i))
                                {
                                    case "cTitulo":
                                        rlivro.Titulo = reader.GetFieldValue<string>(i);
                                        break;
                                    case "cAutor":
                                        rlivro.Autor = reader.GetFieldValue<string>(i);
                                        break;
                                    case "cGenero":
                                        rlivro.Genero = GetEnumValue<Generos>(reader.GetFieldValue<string>(i));
                                        break;
                                    case "nPreco":
                                        rlivro.Preco = reader.GetFieldValue<double>(i);
                                        break;
                                    case "nQuantidade":
                                        rlivro.Quantidade = reader.GetFieldValue<double>(i);
                                        break;
                                    case "Date":
                                        rlivro.Date = reader.GetFieldValue<DateOnly>(i);
                                        break;
                                    case "id":
                                        rlivro.Id = reader.GetFieldValue<int>(i);
                                        break;
                                }
                            }

                            rlivros.Add(rlivro);

                        }
                    } while (reader.NextResult());
                    if (conn.State != System.Data.ConnectionState.Closed)
                        conn.Close();
                    return (null, rlivros, null);
                }
                //return rlivros;
            }
            catch (Exception ex)
            {
                Exception ex1 = ex;
                throw ex1;
            }
            return (null,null, " Nao encontrado");
        }

        public static (Livros?, List<Livros>?, string?) Update(object lista)
        {
            try
            {
                List<Livros>? livros = lista as List<Livros>;
                List<Livros> rlivros = [];
                if (lista.GetType() == typeof(Livros))
                {
                    using (var conn = DbConnection())
                    {
                        if (conn.State == System.Data.ConnectionState.Closed)
                            conn.Open();
                        var cmd = conn.CreateCommand();

                        if (lista is Livros livro && livro.Id != 0)
                        {
                            cmd.CommandText = "UPDATE Livros SET cTitulo=@cTitulo, cAutor=@cAutor, cGenero=@cGenero, nPreco=@nPreco, nQuantidade=@nQuantidade, Date=@Date WHERE id=@Id RETURNING *";
                            cmd.Parameters.AddWithValue("@Id", livro.Id);
                            cmd.Parameters.AddWithValue("@cTitulo", livro.Titulo);
                            cmd.Parameters.AddWithValue("@cAutor", livro.Autor);
                            cmd.Parameters.AddWithValue("@cGenero", livro.Genero.ToString());
                            cmd.Parameters.AddWithValue("@nPreco", livro.Preco);
                            cmd.Parameters.AddWithValue("@nQuantidade", livro.Quantidade);
                            cmd.Parameters.AddWithValue("@Date", livro.Date);
                            //cmd.ExecuteNonQuery();
                            using var reader = cmd.ExecuteReader();
                            do
                            {
                                while (reader.Read())
                                {

                                    var rlivro = new Livros(DateOnly.FromDateTime(DateTime.Now));


                                    for (int i = 0; i < reader.FieldCount; i++)
                                    {
                                        switch (reader.GetName(i))
                                        {
                                            case "cTitulo":
                                                rlivro.Titulo = reader.GetFieldValue<string>(i);
                                                break;
                                            case "cAutor":
                                                rlivro.Autor = reader.GetFieldValue<string>(i);
                                                break;
                                            case "cGenero":
                                                rlivro.Genero = GetEnumValue<Generos>(reader.GetFieldValue<string>(i));
                                                break;
                                            case "nPreco":
                                                rlivro.Preco = reader.GetFieldValue<double>(i);
                                                break;
                                            case "nQuantidade":
                                                rlivro.Quantidade = reader.GetFieldValue<double>(i);
                                                break;
                                            case "Date":
                                                rlivro.Date = reader.GetFieldValue<DateOnly>(i);
                                                break;
                                            case "id":
                                                rlivro.Id = reader.GetFieldValue<int>(i);
                                                break;
                                        }
                                    }

                                    if (conn.State != System.Data.ConnectionState.Closed)
                                        conn.Close();
                                    return (rlivro, null, null);

                                }
                            } while (reader.NextResult());
                        }
                        else
                            return (null, null, " Id nao informado ou Livro nullo");
                    };
                }
                else if (lista.GetType() == typeof(List<Livros>))
                {
                    using var conn = DbConnection();
                    Livros? livro = lista as Livros;
                    if (conn.State == System.Data.ConnectionState.Closed)
                        conn.Open();
                    var cmd = conn.CreateCommand();

                    string jsonString = JsonSerializer.Serialize(livros);
                    //--SELECT JSO.id,JSO.autor,JSO.genero,JSO.titulo,JSO.date,JSO.preco,JSO.quantidade, Liv.cAutor, Liv.cGenero, Liv.cTitulo, Liv.Date, Liv.nPreco, Liv.nQuantidade
                    cmd.CommandText = " UPDATE Livros SET ";
                    cmd.CommandText += "   cAutor =CASE WHEN JSO.autor IS NULL THEN Liv.cAutor ELSE JSO.autor END, ";
                    cmd.CommandText += "   cGenero =CASE WHEN JSO.genero IS NULL THEN Liv.cGenero ELSE JSO.genero END, ";
                    cmd.CommandText += "   cTitulo =CASE WHEN JSO.titulo IS NULL THEN Liv.cTitulo ELSE JSO.titulo END, ";
                    cmd.CommandText += "   Date =CASE WHEN JSO.date IS NULL THEN Liv.Date ELSE JSO.date END, ";
                    cmd.CommandText += "   nPreco =CASE WHEN JSO.preco IS NULL THEN Liv.nPreco ELSE JSO.preco END, ";
                    cmd.CommandText += "   nQuantidade =CASE WHEN JSO.quantidade IS NULL THEN Liv.nQuantidade ELSE JSO.quantidade END ";
                    cmd.CommandText += " FROM( ";
                    cmd.CommandText += "    SELECT  ";
                    cmd.CommandText += "         json_extract(json_each.value, '$.Id') AS id ";
                    cmd.CommandText += "        , json_extract(json_each.value, '$.Autor') AS autor ";
                    cmd.CommandText += "        , json_extract(json_each.value, '$.Genero') AS genero ";
                    cmd.CommandText += "        , json_extract(json_each.value, '$.Titulo') AS titulo ";
                    cmd.CommandText += "        , json_extract(json_each.value, '$.Preco') AS preco ";
                    cmd.CommandText += "        , json_extract(json_each.value, '$.Quantidade') AS quantidade ";
                    cmd.CommandText += "        , json_extract(json_each.value, '$.Date') AS date ";
                    cmd.CommandText += "        ,json_each.value ";
                    cmd.CommandText += "    FROM json_each(' ";
                    cmd.CommandText += jsonString;
                    cmd.CommandText += "    ') ";
                    cmd.CommandText += " ) JSO ";
                    cmd.CommandText += " INNER JOIN Livros Liv ON Liv.id=JSO.id ";
                    cmd.CommandText += " WHERE Livros.id=Liv.id  AND JSO.id <> 0 AND JSO.id IS NOT NULL ";
                    cmd.CommandText += " RETURNING * ";

                    using var reader = cmd.ExecuteReader();
                    do
                    {
                        while (reader.Read())
                        {
                            var rlivro = new Livros(DateOnly.FromDateTime(DateTime.Now));

                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                switch (reader.GetName(i))
                                {
                                    case "cTitulo":
                                        rlivro.Titulo = reader.GetFieldValue<string>(i);
                                        break;
                                    case "cAutor":
                                        rlivro.Autor = reader.GetFieldValue<string>(i);
                                        break;
                                    case "cGenero":
                                        rlivro.Genero = GetEnumValue<Generos>(reader.GetFieldValue<string>(i));
                                        break;
                                    case "nPreco":
                                        rlivro.Preco = reader.GetFieldValue<double>(i);
                                        break;
                                    case "nQuantidade":
                                        rlivro.Quantidade = reader.GetFieldValue<double>(i);
                                        break;
                                    case "Date":
                                        rlivro.Date = reader.GetFieldValue<DateOnly>(i);
                                        break;
                                    case "id":
                                        rlivro.Id = reader.GetFieldValue<int>(i);
                                        break;
                                }
                            }

                            rlivros.Add(rlivro);

                        }
                    } while (reader.NextResult());
                    if (conn.State != System.Data.ConnectionState.Closed)
                        conn.Close();
                    return (null, rlivros, null);
                }
            }
            catch (Exception ex)
            {
                Exception ex1 = ex;
                throw ex1;
            }
            return (null, null, " Nao encontrado");
        }

        [HttpDelete()]
        [Route("{id}")]
        [ProducesResponseType(typeof(String), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(String), StatusCodes.Status400BadRequest)]
        public IActionResult DeleteLivro([FromRoute] int id)
        {
            if (id == 0)
                return NotFound("Id Invalido.");

            CriarTabelaSQlite();

            var (cok, errou) = Delete(id);

            return (cok != null) ? Ok(cok) : ((errou != null) ? NotFound(errou) : NotFound("falhou"));

        }
        public static (String?, String?) Delete(int Id)
        {
            try
            {
                int nOk;
                using (var conn = DbConnection())
                {
                    if (conn.State == System.Data.ConnectionState.Closed)
                        conn.Open();
                    var cmd = conn.CreateCommand();
                    cmd.CommandText = "DELETE FROM Livros Where id=@Id";
                    cmd.Parameters.AddWithValue("@Id", Id);
                    nOk = cmd.ExecuteNonQuery();
                    if (conn.State != System.Data.ConnectionState.Closed)
                        conn.Close();
                }
                if (nOk == 1)
                    return (" Id "+ Id.ToString() +" Excluído!", null);
                else
                    return (null, " Item Nao encontrado");
            }
            catch (Exception ex)
            {
                Exception ex1 = ex;
                throw ex1;
                
            }
        }
    }
}
