var dataTable;

$(document).ready(function () {
    loadTableData()
});

function loadTableData(){
    dataTable=$('#tblData').DataTable({
        "ajax": {
            "url":"/Admin/Category/GetAll"
        },
        "columns": [
            { "data": "name", "width": "70%" },
            {
                "data": "id",
                "render": function (data) {
                    return `
                <div class="text-center">
                <a href="/Admin/Category/Upsert/${data}" class="btn btn-info"><i class="fas fa-edit"></i></a>
                <a onclick=Delete("/Admin/Category/Delete/${data}") class="btn btn-danger"><i class="fas fa-trash"></i></a>
                </div>`;

                }
                }
            ]
        })
}
function Delete(url) {
    swal({
        title: "Want To Delete",
        text: "Data Delete!!!",
        icon: "warning",
        buttons: true,
        dangerModel: true

    }).then((willDelete) => {
        if (willDelete) {
            $.ajax({
                url: url,
                type: "DELETE",
                success: function (data) {
                    if (data.success) {
                        dataTable.ajax.reload();
                        toastr.success(data.message)
                    }
                    else {
                        toastr.error(data.message)
                    }

                }
            })
        }


    })
}
