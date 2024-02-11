var dataTable;

$(document).ready(function () {
    loadTableData()
});

function loadTableData() {
    dataTable = $('#tblData').DataTable({
        "ajax": {
            "url": "/Admin/Product/GetAll"
        },
        "columns": [
            { "data": "title", "width": "15%" },
            { "data": "author", "width": "15%" },
            { "data": "description", "width": "20%" },
            { "data": "isbn", "width": "15%" },
            { "data": "price", "width": "15%" },
            {
                "data": "id",
                "render": function (data) {
                    return `
                    <div class="text-center">
                     <a href="/Admin/Product/Upsert/${data}" class="btn btn-info"><i class="fas fa-edit"></i></a>
                     <a onclick=Delete("/Admin/Product/Delete/${data}") class="btn btn-danger"><i class="fas fa-trash"></i></a>
                    </div>
                    `;
                }
            }
        ]
    })

}
function Delete(url) {
    swal({
        title: "Want To Delete Book?",
        text: "Product Will Delete!!!",
        icon: "warning",
        buttons: true,
        dangerModel:true
    }).then((willDelete) => {
        if (willDelete) {
            $.ajax({
                url: url,
                type: "DELETE",
                success: function (data) {
                    if (data.success) {
                        dataTable.ajax.reload()
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