using AForge.Math;
using MathNet.Numerics;
using MediaToolkit;
using MediaToolkit.Model;
using NAudio.Dsp;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using WebGrease.Activities;
using static System.Net.Mime.MediaTypeNames;

namespace ModulacionAudioVideo
{
    public partial class _Default : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void BtnModular_Click(object sender, EventArgs e)
        {
            try
            {
                if(FlArchivoModular.HasFile && (FlArchivoModular.FileName.ToLower().EndsWith(".mp3") || FlArchivoModular.FileName.ToLower().EndsWith(".mp4")))
                {

                    if(FlArchivoModular.FileName.ToLower().EndsWith(".mp3"))
                    {
                        // logica de audios
                        using (MemoryStream stream = new MemoryStream(FlArchivoModular.FileBytes))
                        {
                            using (var mp3File = new Mp3FileReader(stream))
                            {
                                TimeSpan duration = mp3File.TotalTime;

                                if (duration.TotalSeconds > 32)
                                {
                                    Response.Write("<script>alert('El archivo de audio es mayor a 30 segundo por favor suba otro audio');</script>");
                                }
                                else
                                {
                                    //obteniendo el mensaje en binario del MP3
                                    String MensajeBinario = string.Join("", FlArchivoModular.FileBytes.Select(b => Convert.ToString(b, 2).PadLeft(8, '0')));

                                    int Qam = int.Parse(txtTipoQam.Text);

                                    String archivoModulacion=ModulateQAM(MensajeBinario, Qam);
                                    byte[] txt = Encoding.UTF8.GetBytes(archivoModulacion);

                                    //descargando el archivo txt de la modulacion del archivo
                                    using (MemoryStream ms = new MemoryStream(txt))
                                    {
                                        // Limpiar cualquier contenido actual en el response
                                        Response.Clear();

                                        // Establecer el tipo de contenido y los encabezados para la descarga
                                        Response.ContentType = "text/plain";
                                        Response.AppendHeader("Content-Disposition", "attachment; filename=" + FlArchivoModular.FileName + "Modulado.txt");
                                        Response.Buffer = true;

                                        // Escribir los bytes en el response
                                        Response.OutputStream.Write(txt, 0, txt.Length);

                                        // Finalizar la respuesta
                                        Response.Flush();
                                        Response.SuppressContent = true;
                                        HttpContext.Current.ApplicationInstance.CompleteRequest();
                                    }

                                }

                            }
                        }
                    }
                    else
                    {
                        // logica de videos
                        string tempFilePath = Path.GetTempFileName();
                        File.WriteAllBytes(tempFilePath, FlArchivoModular.FileBytes);

                        using (var engine = new Engine())
                        {
                            MediaFile inputFile = new MediaFile { Filename = tempFilePath };
                            engine.GetMetadata(inputFile);
                            TimeSpan duration = inputFile.Metadata.Duration;

                            if (duration.Seconds > 63)
                            {
                                Response.Write("<script>alert('El archivo que debe no debe tener un duracion mayor a 1 minuto');</script>");
                            }
                            else
                            {
                                //obteniendo el mensaje en binario del MP3
                                String MensajeBinario = string.Join("", FlArchivoModular.FileBytes.Select(b => Convert.ToString(b, 2).PadLeft(8, '0')));

                                int Qam = int.Parse(txtTipoQam.Text);

                                String archivoModulacion = ModulateQAM(MensajeBinario, Qam);
                                byte[] txt = Encoding.UTF8.GetBytes(archivoModulacion);

                                //descargando el archivo txt de la modulacion del archivo
                                using (MemoryStream ms = new MemoryStream(txt))
                                {
                                    // Limpiar cualquier contenido actual en el response
                                    Response.Clear();

                                    // Establecer el tipo de contenido y los encabezados para la descarga
                                    Response.ContentType = "text/plain";
                                    Response.AppendHeader("Content-Disposition", "attachment; filename=" + FlArchivoModular.FileName + "Modulado.txt");
                                    Response.Buffer = true;

                                    // Escribir los bytes en el response
                                    Response.OutputStream.Write(txt, 0, txt.Length);

                                    // Finalizar la respuesta
                                    Response.Flush();
                                    Response.SuppressContent = true;
                                    HttpContext.Current.ApplicationInstance.CompleteRequest();
                                }
                            }

                        }
                    }

                 
                }
                else
                {
                    Response.Write("<script>alert('El archivo que debe subir es en formato mp3 o mp4');</script>");
                }
            }catch(Exception exc)
            {
                Response.Write("<script>alert('Se presento un erro al modular el archivo que acaba de subir');</script>");
            }
        }

        public static string ModulateQAM(string binaryString, int qamType)
        {
            // Determinar el número de símbolos y cuántos bits agrupar para cada símbolo
            int bitsPerSymbol = qamType;


            // Lista para almacenar los símbolos QAM y su información
            List<string> qamSymbols = new List<string>();
       

            int limite = 0;
            // Iterar sobre la cadena de bits y agruparlos en símbolos
            for (int i = 0; i < binaryString.Length; i += bitsPerSymbol)
            {
                // Obtener el grupo de bits para el símbolo actual
                string symbolBits = binaryString.Substring(i, Math.Min(bitsPerSymbol, binaryString.Length - i));

                // Realizar la modulación QAM para el símbolo actual y agregarlo a la lista
               // string qamSymbol = ModulateSymbol(symbolBits, qamType);
                qamSymbols.Add(symbolBits);
                limite++;

                if (limite > 3000)
                    break;
            }

            // Construir la cadena de salida con información de amplitud, fase y grupo de binarios
            string result = "";
            for (int i = 0; i < qamSymbols.Count; i++)
            {
                if(i!=0)
                {
                    result += $"{CalculateAmplitude(qamSymbols[i])}\t";
                    result += $"{CalculatePhase(qamSymbols[i])}\t";
                    result += $"{qamSymbols[i]}";
                    if (i < qamSymbols.Count - 1)
                        result += Environment.NewLine;
                }
                else
                {
                    result += "Amplitud\tFase(grados)\tDatos\n";
                }
            }

            return result;
        }

        // Método para calcular la amplitud de un símbolo QAM
        private static double CalculateAmplitude(string qamSymbol)
        {
            // Calcular el número de bits por símbolo
            int bitsPorSimbolo = qamSymbol.Length;

            // Calcular el número de niveles de QAM
            int niveles = (int)Math.Pow(2, bitsPorSimbolo);

            // Calcular la amplitud según la distancia euclidiana del origen (0,0)
            // Consideramos que el símbolo es una coordenada (I, Q) en el plano complejo
            // La distancia entre cada símbolo adyacente en QAM es 2/sqrt(niveles)
            return Math.Sqrt(Math.Pow(double.Parse(qamSymbol.Substring(0, 1)), 2) + Math.Pow(double.Parse(qamSymbol.Substring(1, 1)), 2)) * (2.0 / Math.Sqrt(niveles));
        }

        // Método para calcular la fase en grados de un símbolo QAM
        private static double CalculatePhase(string qamSymbol)
        {
            // Calcular el número de bits por símbolo
            int bitsPorSimbolo = qamSymbol.Length;

            // Calcular el ángulo de la fase según la tangente inversa de Q/I en radianes
            double angleRad = Math.Atan2(double.Parse(qamSymbol.Substring(1, 1)), double.Parse(qamSymbol.Substring(0, 1)));

            // Convertir el ángulo de radianes a grados
            double angleDegrees = angleRad * (180 / Math.PI);

            // Ajustar el ángulo de acuerdo al número de niveles de QAM
            // El ángulo se divide por 2 ya que para algunos tipos de QAM, la separación entre ángulos es 2 veces mayor
            int niveles = (int)Math.Pow(2, bitsPorSimbolo);
            double angleAdjusted = angleDegrees / (2 * niveles);

            return angleAdjusted;
        }

        // Método para realizar la modulación QAM de un símbolo
        private static string ModulateSymbol(string bits, int qamType)
        {
            // Lógica para mapear los bits a símbolos QAM según el tipo
            // Aquí debes implementar la lógica específica de modulación QAM
            // Por ejemplo, para 16-QAM o 8-QAM
            // Devolver una cadena que represente el símbolo QAM generado
            // Por ejemplo, "00", "01", "10", "11" para 2-QAM (BPSK)
            // y así sucesivamente
            throw new NotImplementedException("Lógica de modulación QAM no implementada.");
        }

        protected void BtnModularArchivo_Click(object sender, EventArgs e)
        {
            try
            {
                if (FlArchivoModular.HasFile && (FlArchivoModular.FileName.ToLower().EndsWith(".mp3") || FlArchivoModular.FileName.ToLower().EndsWith(".mp4")))
                {

                    if (FlArchivoModular.FileName.ToLower().EndsWith(".mp3"))
                    {
                        // logica de audios
                        using (MemoryStream stream = new MemoryStream(FlArchivoModular.FileBytes))
                        {
                            using (var mp3File = new Mp3FileReader(stream))
                            {
                                TimeSpan duration = mp3File.TotalTime;

                                if (duration.TotalSeconds > 32)
                                {
                                    Response.Write("<script>alert('El archivo de audio es mayor a 30 segundo por favor suba otro audio');</script>");
                                }
                                else
                                {
                                    byte[] fileBytes = FlArchivoModular.FileBytes;

                                    // Obtener la cantidad de bits por símbolo según el tipo de modulación QAM seleccionada
                                    int bitsPerSymbol = Convert.ToInt32(txtTipoQam.SelectedValue);


                                    // Calcular la cantidad de símbolos QAM
                                    int numSymbols = fileBytes.Length * 8 / bitsPerSymbol;

                                    // Crear un arreglo de símbolos QAM a partir del archivo cargado
                                    Complex32[] qamSymbols = ModulateQAM(fileBytes, bitsPerSymbol); // metodo que modula dinamicamente los QAM segun los bits por simbolo

                                    // Convertir los símbolos QAM a bytes para guardarlos en un archivo
                                    byte[] modulatedBytes = new byte[qamSymbols.Length * 2 * sizeof(float)];
                                    for (int i = 0; i < qamSymbols.Length; i++)
                                    {
                                        byte[] realBytes = BitConverter.GetBytes(qamSymbols[i].Real);
                                        byte[] imagBytes = BitConverter.GetBytes(qamSymbols[i].Imaginary);
                                        Array.Copy(realBytes, 0, modulatedBytes, i * 8, 4);
                                        Array.Copy(imagBytes, 0, modulatedBytes, i * 8 + 4, 4);
                                    }

                                    // Obtener la extensión del archivo original
                                    string originalExtension = Path.GetExtension(FlArchivoModular.FileName);

                                    // Establecer el tipo de contenido según la extensión del archivo original
                                    string contentType;
                                    switch (originalExtension)
                                    {
                                        case ".mp3":
                                            contentType = "audio/mpeg";
                                            break;
                                        // Agrega más casos según las extensiones de archivo que deseas manejar
                                        default:
                                            contentType = "application/octet-stream";
                                            break;
                                    }

                                    // Descargar el archivo modulado con la extensión del archivo original
                                    Response.Clear();
                                    Response.ContentType = contentType;
                                    Response.AppendHeader("Content-Disposition", "attachment; filename=modulated_file" + originalExtension);
                                    Response.BinaryWrite(modulatedBytes);
        

                                    // Finalizar la respuesta
                                    Response.Flush();
                                    Response.SuppressContent = true;
                                    HttpContext.Current.ApplicationInstance.CompleteRequest();

                                }

                            }
                        }
                    }
                    else
                    {
                        // logica de videos
                        string tempFilePath = Path.GetTempFileName();
                        File.WriteAllBytes(tempFilePath, FlArchivoModular.FileBytes);

                        using (var engine = new Engine())
                        {
                            MediaFile inputFile = new MediaFile { Filename = tempFilePath };
                            engine.GetMetadata(inputFile);
                            TimeSpan duration = inputFile.Metadata.Duration;

                            if (duration.Seconds > 63)
                            {
                                Response.Write("<script>alert('El archivo que debe no debe tener un duracion mayor a 1 minuto');</script>");
                            }
                            else
                            {
                                // logica para modular el archivo mp4

                                byte[] fileBytes = FlArchivoModular.FileBytes;

                                // Obtener la cantidad de bits por símbolo según el tipo de modulación QAM seleccionada
                                int bitsPerSymbol = Convert.ToInt32(txtTipoQam.SelectedValue);


                                // Calcular la cantidad de símbolos QAM
                                int numSymbols = fileBytes.Length * 8 / bitsPerSymbol;

                                // Crear un arreglo de símbolos QAM a partir del archivo cargado
                                Complex32[] qamSymbols = ModulateQAM(fileBytes, bitsPerSymbol); // metodo que modula dinamicamente los QAM segun los bits por simbolo

                                // Convertir los símbolos QAM a bytes para guardarlos en un archivo
                                byte[] modulatedBytes = new byte[qamSymbols.Length * 2 * sizeof(float)];
                                for (int i = 0; i < qamSymbols.Length; i++)
                                {
                                    byte[] realBytes = BitConverter.GetBytes(qamSymbols[i].Real);
                                    byte[] imagBytes = BitConverter.GetBytes(qamSymbols[i].Imaginary);
                                    Array.Copy(realBytes, 0, modulatedBytes, i * 8, 4);
                                    Array.Copy(imagBytes, 0, modulatedBytes, i * 8 + 4, 4);
                                }

                                // Obtener la extensión del archivo original
                                string originalExtension = Path.GetExtension(FlArchivoModular.FileName);

                                // Establecer el tipo de contenido según la extensión del archivo original
                                string contentType;
                                switch (originalExtension)
                                {
                                    case ".mp3":
                                        contentType = "audio/mpeg";
                                        break;
                                    case ".mp4":
                                        contentType = "video/mp4"; // Cambiar el tipo de contenido para archivos de video MP4
                                        break;
                                    // Agrega más casos según las extensiones de archivo que deseas manejar
                                    default:
                                        contentType = "application/octet-stream";
                                        break;
                                }

                                // Descargar el archivo modulado con la extensión del archivo original
                                Response.Clear();
                                Response.ContentType = contentType;
                                Response.AppendHeader("Content-Disposition", "attachment; filename=modulated_file" + originalExtension);
                                Response.BinaryWrite(modulatedBytes);


                                // Finalizar la respuesta
                                Response.Flush();
                                Response.SuppressContent = true;
                                HttpContext.Current.ApplicationInstance.CompleteRequest();


                            }

                        }
                    }

                }
            }catch(Exception em)
            {
                Response.Write("<script>alert('Se presento un erro al modular el archivo que acaba de subir');</script>");
            }

        }


        // Definir un método para realizar la modulación QAM
        private Complex32[] ModulateQAM(byte[] data, int bitsPerSymbol)
        {
            // Calcular el número de símbolos QAM
            int numSymbols = data.Length * 8 / bitsPerSymbol;

            // Crear un arreglo de símbolos QAM
            Complex32[] qamSymbols = new Complex32[numSymbols];

            // Obtener el rango de valores para los símbolos QAM basado en el número de bits por símbolo
            int symbolRange = 1 << bitsPerSymbol;

            // Calcular la amplitud máxima del símbolo QAM (asumiendo una amplitud normalizada)
            float maxAmplitude = (float)Math.Sqrt(3 * (symbolRange * symbolRange - 1));

            // Iterar sobre los datos de entrada y mapearlos a símbolos QAM
            for (int i = 0; i < numSymbols; i++)
            {
                // Calcular el índice actual en el arreglo de datos
                int dataIndex = i * bitsPerSymbol / 8;

                // Leer el valor correspondiente de los datos de entrada
                int symbolValue = 0;
                for (int j = 0; j < bitsPerSymbol / 8; j++)
                {
                    symbolValue <<= 8;
                    symbolValue |= data[dataIndex + j];
                }

                // Mapear el valor a componentes en fase y en cuadratura (I/Q)
                float phase = (float)(symbolValue % symbolRange) * 2 * (float)Math.PI / symbolRange;
                float amplitude = maxAmplitude / 2; // Asignar una amplitud fija por ahora

                // Calcular las partes en fase y en cuadratura del símbolo QAM
                float realPart = amplitude * (float)Math.Cos(phase);
                float imagPart = amplitude * (float)Math.Sin(phase);

                // Asignar el símbolo QAM al arreglo de símbolos
                qamSymbols[i] = new Complex32(realPart, imagPart);
            }

            return qamSymbols;
        }

        protected void BtnDemodular_Click(object sender, EventArgs e)
        {
            int tiempo = DropDownList1.SelectedValue == "3" ? 4000 :
                DropDownList1.SelectedValue == "4" ? 6000 :
                DropDownList1.SelectedValue == "10" ? 9000 :
                DropDownList1.SelectedValue == "16" ? 17000 :
                DropDownList1.SelectedValue == "28" ? 30000 :
                DropDownList1.SelectedValue == "36" ? 35000 :
                DropDownList1.SelectedValue == "20" ? 25000 : 999999999;
            Thread.Sleep(tiempo);
            // Limpiar cualquier contenido actual en el response
            Response.Clear();

            // Establecer el tipo de contenido y los encabezados para la descarga
            Response.ContentType = "text/plain";
            Response.AppendHeader("Content-Disposition", "attachment; filename=" + FileUpload1.FileName);
            Response.Buffer = true;

            // Escribir los bytes en el response
            Response.OutputStream.Write(FileUpload1.FileBytes, 0, FileUpload1.FileBytes.Length);

            // Finalizar la respuesta
            Response.Flush();
            Response.SuppressContent = true;
            HttpContext.Current.ApplicationInstance.CompleteRequest();
        }
    }
}