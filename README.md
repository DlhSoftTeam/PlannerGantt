# Planner-Gantt (beta)
Simple app that displays plans from Office 365 with a Gantt Chart using DlhSoft.GanttChartWebLibrary. 

## Overview
The application uses GanttChartView component from DlhSoft Gantt Chart Web Library to display plans and tasks from Microsoft Office 365 Planner.
It leverages Microsoft Graph Beta API, so it should be used only for testing purposes, and not for production.
Note that at this time it requires administrative account rights for the Office 365 tenant in order to run.

## Running the code
To be able run the code you will need to:

* Create an Azure Web app under your subscription;
* Copy this repository into your application source code;
* Replace {placeholders} inside *.config and *.master files with your own URI, ID, and key values (ensuring you do not publish secrets online):
  * {ApplicationUri} (HTTPS URL)
  * {ClientId} (GUID)
  * {ClientSecret} (Base64 encoded string)
  * {InstrumentationKey} (GUID)
