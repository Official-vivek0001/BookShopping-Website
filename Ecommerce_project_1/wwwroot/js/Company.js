var dataTable;

$(document).ready(function () {
    loadTableData()
});

function loadTableData() {
    dataTable = $('#tblData').DataTable({
        "ajax": {
            "url": "/Admin/Company/GetAll"
        },
        "columns": [
            { "data": "name", "width": "10%" },
            { "data": "streetAddress", "width": "15%" },
            { "data": "city", "width": "10%" },
            { "data": "state", "width": "10%" },
            { "data": "postalCode", "width": "10%" },
            { "data": "phoneNumber", "width": "10%" },
            {
                "data": "isAuthorizedCompany","width":"10", "render": function (data) {
                    if (data == true) {     
                        return `
                        <div class="text-center">
                        <input type="checkbox" name="isAuthorized" checked />
                        </div>`;
                    }
                    else {

                        return `  <div class="text-center">
                        <input type="checkbox" name="isAuthorized" />
                        </div>`;
                    }
                }
                    }, 
            {
                "data": "id",
                "render": function (data) {
                    return `
                    <div class="text-center">
                     <a href="/Admin/Company/Upsert/${data}" class="btn btn-info"><i class="fas fa-edit"></i></a>
                     <a onclick=Delete("/Admin/Company/Delete/${data}") class="btn btn-danger"><i class="fas fa-trash"></i></a>
                    </div>
                    `;
                }
            }
        ]
    })

}
function Delete(url) {
    const swalWithBootstrapButtons = Swal.mixin({
        customClass: {
            confirmButton: 'btn btn-success',
            cancelButton: 'btn btn-danger'
        },
        buttonsStyling: false
    })

    swalWithBootstrapButtons.fire({
        title: 'Are you sure?',
        text: "You won't be able to revert this!",
        icon: 'warning',
        showCancelButton: true,
        confirmButtonText: 'Yes, delete it!',
        cancelButtonText: 'No, cancel!',
        reverseButtons: true
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                url: url,
                type: "DELETE",


                succes: function (data) {

                    swalWithBootstrapButtons.fire.dataTable.ajax.reload()(
                        'Deleted!',
                        'Your file has been deleted.',
                        'success'
                    )
                }



               
            })
        } else if (
            /* Read more about handling dismissals below */
            result.dismiss === Swal.DismissReason.cancel
        ) {
            swalWithBootstrapButtons.fire(
                'Cancelled',
                'Your file is safe :)',
                'error'
            )
        }

    })
}

               

                