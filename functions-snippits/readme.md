Template taken from: https://learn.microsoft.com/en-us/azure/ai-services/openai/how-to/function-calling

Use cases:
1. Request time off
2. Request PTO balance
3. Check on status of expense report
4. Add time to your timesheet
5. Check timesheet balance for the week so far


Workflow Start Events:
1. Request Time Off [url: https://wenning.workflowcloud.com/api/v1/workflow/published/0f181a93-5fca-4214-aa78-305529574b6d/instances?token=AxJOaxRkhMck4zoaeLSVTLNkb2Q1KRVBEErMCoFWtnp4iab355JW0RsKD4t7LkTh0wvly9]
    - numhours: number of hours requested
    - typerofpto : PTO Type
    - startdate: Start Date
    - enddate: End Date
    - email
2. Request PTO Balance [url: https://wenning.workflowcloud.com/api/v1/workflow/published/b66bd70f-58bc-4eec-9567-5b5bcfd52d4d/swagger.json?token=9GFz9paNn8fBwuIfm3FBboY9LUvRgis9x9YX214XL8v7KDNzg1TKLXdPkikwfsJp9R2wa5]
    - email    
3. Check On Status Expense Report [url: https://wenning.workflowcloud.com/api/v1/workflow/published/2bc57363-ed4c-4930-aac5-1191dc535571/instances?token=FqfZmE5oFBlDEXclyJU0T0dldQAuDWwFVxvZ7QWkJ5HZ6ASCdUQb2SUGLc4085AE0nMn7F]
    - Request ID: Expense Report ID
    - Email
4. Add Time to Timesheet [url: https://wenning.workflowcloud.com/api/v1/workflow/published/bea92f56-5777-4e25-8405-d6be020d966a/instances?token=F0wMxkU0gTnniR5JhfF9iHFxFGVAvoR4NH4ZmAW7Pi2mOb9lj2DCFwJNN1FyAucxduhOSr]
    - email
    - numberofhours: Number of Hours
    - date: Date to add hours to
5. Check Timesheet Balance [url: https://wenning.workflowcloud.com/api/v1/workflow/published/0f181a93-5fca-4214-aa78-305529574b6d/instances?token=AxJOaxRkhMck4zoaeLSVTLNkb2Q1KRVBEErMCoFWtnp4iab355JW0RsKD4t7LkTh0wvly9]
    - email
