# CostCheckAF

This is a small Azure function app for Infineon to read Azure resources costs. 

## Usage 

Send GET request to URL: https://ifx-it-azureresourcescostchecker.azurewebsites.net/api/ResCostCheckerAF + add Accepted query parameters: rg=’resource group name’ and/or tag=’key:value’

## Usage example: 

https://ifx-it-azureresourcescostchecker.azurewebsites.net/api/ResCostCheckerAF?rg='IFXBot'

<i>Total consumed cost of the selected resources is: 158.726454884678 EUR</i>
