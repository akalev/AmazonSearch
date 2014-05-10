function SearchByKeyword() {
    $.ajax({
        type: "get",
        url: "/api/pages/page/" + $("#keyword").val(),
        dataType: "xml",
        beforeSend: function () {
            $("#result").html("Searching...");
        },
        success: xmlParser,
        error: function (xhr, status, error) {
            $("#result").html("Server is too busy. Please try again!");
        }
    });
}

function xmlParser(xml) {
    var content = "<thead>\
                        <tr>\
                            <th>Title</th>\
                            <th>Price</th>\
                        </tr>\
                    </thead>\
                    <tbody>";
    $(xml).find("Product").each(function () {
        content += "<tr>\
                        <td>" + $(this).find("Title").text() + "</td>\
                        <td>" + $(this).find("Price").text() + "</td>\
                    </tr>";
    });
    content += "</tbody>";
    $("#result").html(content);
}