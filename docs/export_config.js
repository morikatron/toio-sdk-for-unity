module.exports = {
    stylesheet: [
        "https://cdnjs.cloudflare.com/ajax/libs/github-markdown-css/2.10.0/github-markdown.min.css",
    ],
    css: ".markdown-body {padding: 20px 40px;}",
    body_class: "markdown-body",
    highlight_style: "github",
    marked_options: {},
    pdf_options: {
        format: "A4",
        margin: "20mm",
        printBackground: true,
        // headerTemplate:
        //     '<style>section {margin: 0 auto;font-family: system-ui;font-size: 11px;}</style><section><span class="date"></span></section>',
        // footerTemplate:
        //     '<section><div>Page <span class="pageNumber"></span>of <span class="totalPages"></span></div></section>',
    },
    stylesheet_encoding: "utf-8",
};
