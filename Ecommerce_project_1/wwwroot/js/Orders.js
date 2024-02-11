var dataTable;

$(document).ready(function () {
    loadTableData()
});

function loadTableData() {
    dataTable = $('#tblData').DataTable({
        "ajax": {
            "url": "/Admin/Order/GetAll"
        },
        "columns": [
            { "data": "id", "width": "15%" },
            { "data": "orderDate", "width": "15%" },
            { "data": "orderStatus", "width": "15%" },
            { "data": "name", "width": "15%" },
            { "data": "applicationUser.email", "width": "15%" },
            { "data": "orderTotal", "width": "15%" },
           
            {
                "data": "id",
                "render": function (data) {
                    return `
                    <div class="text-center">
                     <a href="/Admin/Order/Details/${data}" class="btn btn-info"><i class="fas fa-book"></i></a>
                  
                    </div>
                    `;
                }
            }
        ]
    })

}

