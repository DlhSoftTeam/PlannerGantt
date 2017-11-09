# Planner Gantt
Simple Web app that displays Office 365 plans using GanttChartView component from [DlhSoft Gantt Chart Web Library](http://DlhSoft.com/GanttChartWebLibrary). 

## Overview
The application uses GanttChartView component from DlhSoft Gantt Chart Web Library to display plans and tasks from Microsoft Office 365 Planner.
It leverages Microsoft Graph API and might require administrative account rights for the Office 365 tenant in order to run.

## Running the code
To be able run the code you will need to:

* Create an Azure Web app under your subscription;
* Copy this repository into your application source code;
* Replace {placeholders} inside *.config and *.master files with your own URI, ID, and key values (ensuring you do not publish secrets online):
  * {ApplicationUri} (HTTPS URL)
  * {ClientId} (GUID)
  * {ClientSecret} (Base64 encoded string)
  * {InstrumentationKey} (GUID)

## Support
This product is provided for free and "as is", so it doesn't include any official support.
