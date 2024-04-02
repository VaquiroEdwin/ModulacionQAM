<%@ Page Title="Home Page" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="ModulacionAudioVideo._Default" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <main>
        <section class="row" aria-labelledby="aspnetTitle">
            <h1 id="aspnetTitle" class="text-danger">Modulación QAM</h1>
            <p class="lead">
                La modulación es un proceso fundamental en comunicaciones donde se ajusta alguna propiedad de una señal portadora para transportar información. En particular, la modulación de fase (PSK) es un método en el cual la fase de una señal portadora se altera de acuerdo con los datos a enviar, representando así la información en la fase de la señal. Por ejemplo, en PSK binario, se utilizan dos fases para representar bits individuales, mientras que en PSK multivalor, como 1024-QAM, múltiples fases se utilizan para representar múltiples bits simultáneamente, permitiendo una mayor eficiencia espectral.
            </p>
        </section>

        <div class="row">
            <section class="col-md-6" aria-labelledby="gettingStartedTitle">
                <h4 id="gettingStartedTitle" class="text-primary">Modular un archivo de Audio MP3/Video MP4</h4>
                <p class="text-sm text-justify">
                    Para realizar la modulación QAM de un archivo de audio asegurese se subir un archivo de audio en formarto MP3 de
                    una duración no mayor a 30 segundos. Para subir un video, que este sea de formato MP4 y que su duración no sea mayor a 60 seg.
                </p>
                <h6 class="text-primary">Para modular:</h6>
                <div class="row">
                    <div class="col-md-12">
                        <label class="form-label" for="FlArchivoModular">Por favor suba el archivo que desea modular</label>
                        <asp:FileUpload runat="server" ID="FlArchivoModular" CssClass="form-control" />
                    </div>
                </div>
                <div class="row">
                    <div class="col-md-6">
                        <label class="form-label" for="FlArchivoModular">Por favor digite los bits Qam</label>
                        <asp:DropDownList runat="server" ID="txtTipoQam" CssClass="form-select">
                            <asp:ListItem Value="3">8-QAM</asp:ListItem>
                            <asp:ListItem Value="4">16-QAM</asp:ListItem>
                            <asp:ListItem Value="10">1024-QAM</asp:ListItem>
                            <asp:ListItem Value="12">4096-QAM</asp:ListItem>
                            <asp:ListItem Value="16">16bits-QAM</asp:ListItem>
                            <asp:ListItem Value="20">20bits-QAM</asp:ListItem>
                            <asp:ListItem Value="28">28bits-QAM</asp:ListItem>
                            <asp:ListItem Value="36">36bits-QAM</asp:ListItem>
                            <asp:ListItem Value="64">64bits-QAM</asp:ListItem>
                        </asp:DropDownList>
                    </div>
                </div>

                <div class="row mt-4">
                    <div class="col-md-12 text-center">
                        <asp:Button runat="server" ID="BtnModularArchivo" Text="Obtener Archivo Modulado" CssClass="btn btn-success" OnClick="BtnModularArchivo_Click" />

                        <asp:Button runat="server" ID="BtnModular" Text="Modular Archivo" CssClass="btn btn-success" OnClick="BtnModular_Click" />
                    </div>
                </div>

            </section>
            <section class="col-md-6" aria-labelledby="librariesTitle">
                <h4 id="librariesTitle" class=" text-warning">Demodulacion QAM</h4>
                <p class="text-sm text-justify">
                    Para demodular el archivo solo tiene que subirlo, asegurese de haberlo modulado y seleccione con que tipo de QAM lo debe demodular, recuerde que sin oselecciona el correcto no va a funcionar el archivo.
                </p>
                <h6 id="kkk" class=" text-warning">Demodulando</h6>
                <div class="row">
                    <div class="col-md-6">
                        <label class="form-label" for="FlArchivoModular">Por favor suba el archivo modulado</label>
                        <asp:FileUpload runat="server" ID="FileUpload1" CssClass="form-control" />
                    </div>

                    <div class="col-md-6">
                        <label class="form-label" for="FlArchivoModular">Por favor digite los bits Qam</label>
                        <asp:DropDownList runat="server" ID="DropDownList1" CssClass="form-select">
                            <asp:ListItem Value="3">8-QAM</asp:ListItem>
                            <asp:ListItem Value="4">16-QAM</asp:ListItem>
                            <asp:ListItem Value="10">1024-QAM</asp:ListItem>
                            <asp:ListItem Value="12">4096-QAM</asp:ListItem>
                            <asp:ListItem Value="16">16bits-QAM</asp:ListItem>
                            <asp:ListItem Value="20">20bits-QAM</asp:ListItem>
                            <asp:ListItem Value="28">28bits-QAM</asp:ListItem>
                            <asp:ListItem Value="36">36bits-QAM</asp:ListItem>
                            <asp:ListItem Value="64">64bits-QAM</asp:ListItem>
                        </asp:DropDownList>
                    </div>
                </div>

                <div class="row mt-4">
                    <div class="col-md-12 text-center">
                        <asp:Button runat="server" ID="BtnDemodular" Text="Demodular Archivo" CssClass="btn btn-success" OnClick="BtnDemodular_Click" />
                    </div>
                </div>
            </section>
        </div>
    </main>

</asp:Content>
