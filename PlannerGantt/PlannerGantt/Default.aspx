<%@ Page Title="Planner Gantt" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="PlannerGantt.DefaultPage" %>
<%@ Register TagPrefix="pdgcc" Namespace="DlhSoft.Web.UI.WebControls" Assembly="DlhSoft.ProjectData.GanttChart.ASP.Controls" %>
<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <script src="/Scripts/templates.js"></script>
    <script src="/Scripts/themes.js"></script>
    <div class="row">
        <div class="col-md-12" style="padding-top: 20px">
            <pdgcc:GanttChartView ID="GanttChartView" runat="server" IsReadOnly="True"/>
        </div>
    </div>
</asp:Content>
