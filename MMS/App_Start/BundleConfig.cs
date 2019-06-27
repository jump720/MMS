using System.Web;
using System.Web.Optimization;

namespace MMS
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));


            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            //JS Materialize
            bundles.Add(new ScriptBundle("~/bundles/Materialize").Include(
                        "~/Scripts/Materialize/materialize.min.js"));

            //JS Seguridad
            bundles.Add(new ScriptBundle("~/bundles/Seguridad").Include(
                        "~/Scripts/Seguridad/Layout.js"));

            //CSS Materialize
            bundles.Add(new StyleBundle("~/Content/Materialize").Include(
                      "~/Content/Materialize/materialize.min.css"));


          
            //JS DropZone
            bundles.Add(new ScriptBundle("~/bundles/Dropzone").Include(
                        "~/Scripts/dropzone/dropzone.js"));

            // CSS DropZone
            bundles.Add(new StyleBundle("~/Content/Dropzone").Include(
                                        "~/Scripts/dropzone/dropzone.css"));


            #region Validate

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                "~/Content/dist/plugins/jquery-validation/jquery.validate.min.js",
                "~/Content/dist/plugins/jquery-validation/jquery.validate.unobtrusive.min.js",
                "~/Content/dist/plugins/jquery-validation/jquery.validate.reset.js"
                ));
            #endregion

            #region DataTables

            bundles.Add(new StyleBundle("~/bundles/cssDataTables").Include(
                "~/Content/dist/plugins/jquery-datatable/skin/bootstrap/css/dataTables.bootstrap.css"
                ));

            bundles.Add(new StyleBundle("~/bundles/cssDataTablesResponsive").Include(
              "~/Content/dist/plugins/jquery-datatable/responsive.dataTables.min.css"
              ));

            bundles.Add(new ScriptBundle("~/bundles/jsDataTables").Include(
                "~/Content/dist/plugins/jquery-datatable/jquery.dataTables.js",
                "~/Content/dist/plugins/jquery-datatable/extensions/dataTables.fnFilterOnReturn.js",
                "~/Content/dist/plugins/jquery-datatable/extensions/dataTables.reset.js",
                "~/Content/dist/plugins/jquery-datatable/skin/bootstrap/js/dataTables.bootstrap.js"
                ));

            bundles.Add(new ScriptBundle("~/bundles/jsDataTablesResponsive").Include(
               "~/Content/dist/plugins/jquery-datatable/dataTables.responsive.min.js"
               ));

            #endregion

            #region Select2

            bundles.Add(new StyleBundle("~/bundles/cssSelect2").Include(
                       "~/Content/dist/plugins/select2/select2.min.css",
                       "~/Content/dist/plugins/select2/select2-bootstrap.min.css"
                       ));

            bundles.Add(new ScriptBundle("~/bundles/jsSelect2").Include(
                      "~/Content/dist/plugins/select2/select2.full.min.js"
                      ));

            #endregion

            #region VueTooltip

            bundles.Add(new StyleBundle("~/bundles/cssVueTooltip").Include(
                       "~/Content/dist/plugins/vue/v-tooltip.css"
                       ));

            bundles.Add(new ScriptBundle("~/bundles/jsVueTooltip").Include(
                      "~/Content/dist/plugins/vue/tether.js",
                      "~/Content/dist/plugins/vue/tether-drop.js",
                      "~/Content/dist/plugins/vue/tether-tooltip.js",
                      "~/Content/dist/plugins/vue/v-tooltip.js"
                      ));

            #endregion

            #region Vue

#if DEBUG
            bundles.Add(new ScriptBundle("~/bundles/jsVue").Include(
                      "~/Content/dist/plugins/vue/vue.js"
                      ));
#else
            bundles.Add(new ScriptBundle("~/bundles/jsVue").Include(
                      "~/Content/dist/plugins/vue/vue.min.js"
                      ));
#endif

            #endregion

            #region tagsinput
            //CSS bootstrap-tagsinput
            bundles.Add(new StyleBundle("~/Content/TagsInput").Include(
                      "~/Content/dist/plugins/bootstrap-tagsinput/bootstrap-tagsinput.css"));

            //JS bootstrap-tagsinput
            bundles.Add(new ScriptBundle("~/bundles/TagsInput").Include(
                      "~/Content/dist/plugins/bootstrap-tagsinput/bootstrap-tagsinput.js"
                      ));

            #endregion

            #region TimeLine

            bundles.Add(new StyleBundle("~/bundles/TimeLine").Include(
                       "~/Content/css/TimeLine.css"
                       ));


            #endregion

            #region CKEditor


            bundles.Add(new ScriptBundle("~/bundles/ckeditor").Include(
                      "~/Content/dist/plugins/ckeditor/ckeditor.js"
                      ));

            #endregion CKEditor

            #region Jquery DataTable Export


            //bundles.Add(new ScriptBundle("~/bundles/jsDataTablesExport").Include(
            //  "~/Content/dist/plugins/jquery-datatable/extensions/export/dataTables.buttons.min.js",
            //  "~/Content/dist/plugins/jquery-datatable/extensions/export/buttons.flash.min.js",
            //  "~/Content/dist/plugins/jquery-datatable/extensions/export/jszip.min.js",
            //  "~/Content/dist/plugins/jquery-datatable/extensions/export/pdfmake.min.js",
            //  "~/Content/dist/plugins/jquery-datatable/extensions/export/vfs_fonts.js",
            //  "~/Content/dist/plugins/jquery-datatable/extensions/export/buttons.html5.min.js",
            //  "~/Content/dist/plugins/jquery-datatable/extensions/export/buttons.print.min.js"


            //  ));

            #endregion

            #region lightGallery

            bundles.Add(new StyleBundle("~/content/lightGallery").Include(
                       "~/Content/dist/plugins/light-gallery/css/lightgallery.css"
                       ));

            bundles.Add(new ScriptBundle("~/bundles/lightGallery").Include(
                     "~/Content/dist/plugins/light-gallery/js/lightgallery-all.js"
                     ));

            #endregion

        }
    }
}
