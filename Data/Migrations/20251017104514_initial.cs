using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Newtonsoft.Json.Linq;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:admin_user_role", "none,super_user,developer,data_entry,support");

            migrationBuilder.CreateTable(
                name: "admin_users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    first_name = table.Column<string>(type: "text", nullable: true),
                    last_name = table.Column<string>(type: "text", nullable: true),
                    username = table.Column<string>(type: "text", nullable: true),
                    password = table.Column<string>(type: "text", nullable: true),
                    roles = table.Column<int[]>(type: "integer[]", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_admin_users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "data_source_json_pairs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    cloned_from_id = table.Column<Guid>(type: "uuid", nullable: true),
                    cloner = table.Column<string>(type: "text", nullable: true),
                    source = table.Column<JToken>(type: "jsonb", nullable: true),
                    dst = table.Column<JToken>(type: "jsonb", nullable: true),
                    never_fact = table.Column<List<object>>(type: "jsonb[]", nullable: true),
                    good_fact = table.Column<List<object>>(type: "jsonb[]", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_data_source_json_pairs", x => x.id);
                    table.ForeignKey(
                        name: "fk_data_source_json_pairs_data_source_json_pairs_cloned_from_id",
                        column: x => x.cloned_from_id,
                        principalTable: "data_source_json_pairs",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "history",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    admin_id = table.Column<Guid>(type: "uuid", nullable: true),
                    entity_name = table.Column<string>(type: "text", nullable: true),
                    entity_id = table.Column<Guid>(type: "uuid", nullable: false),
                    data = table.Column<JToken>(type: "jsonb", nullable: true),
                    dif = table.Column<JToken>(type: "jsonb", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_history", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "history2",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    admin_id = table.Column<Guid>(type: "uuid", nullable: true),
                    entity_name = table.Column<string>(type: "text", nullable: true),
                    entity_id = table.Column<int>(type: "integer", nullable: false),
                    data = table.Column<JToken>(type: "jsonb", nullable: true),
                    dif = table.Column<JToken>(type: "jsonb", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_history2", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "news",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    platforms = table.Column<int[]>(type: "integer[]", nullable: true),
                    user_register_min_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    user_register_max_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    end_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    notif_type = table.Column<int>(type: "integer", nullable: false),
                    title = table.Column<string>(type: "text", nullable: false),
                    content = table.Column<string>(type: "text", nullable: false),
                    files = table.Column<List<string>>(type: "text[]", nullable: true),
                    on_arive_method = table.Column<int>(type: "integer", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    start_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    is_removed = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_news", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "payments",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    tracking_number = table.Column<long>(type: "bigint", nullable: false),
                    amount = table.Column<decimal>(type: "numeric", nullable: false),
                    token = table.Column<string>(type: "text", nullable: true),
                    transaction_code = table.Column<string>(type: "text", nullable: true),
                    gateway_name = table.Column<string>(type: "text", nullable: true),
                    gateway_account_name = table.Column<string>(type: "text", nullable: true),
                    is_completed = table.Column<bool>(type: "boolean", nullable: false),
                    is_paid = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_payments", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "profiles",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "text", nullable: true),
                    level = table.Column<int>(type: "integer", nullable: false),
                    disease = table.Column<string>(type: "text", nullable: true),
                    avatar = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_profiles", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "send_sms_histories",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    phone_number = table.Column<string>(type: "character varying(13)", maxLength: 13, nullable: true),
                    ip_address = table.Column<byte[]>(type: "bytea", maxLength: 16, nullable: true),
                    send_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    code = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_send_sms_histories", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "transactions",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    amount = table.Column<decimal>(type: "numeric", nullable: false),
                    type = table.Column<byte>(type: "smallint", nullable: false),
                    is_succeed = table.Column<bool>(type: "boolean", nullable: false),
                    message = table.Column<string>(type: "text", nullable: true),
                    additional_data = table.Column<string>(type: "text", nullable: true),
                    payment_id = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_transactions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "user_details",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uuid", maxLength: 450, nullable: false),
                    first_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    sur_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    required_mark = table.Column<int>(type: "integer", nullable: false),
                    month_to_exam = table.Column<int>(type: "integer", nullable: false),
                    profile_image_base64 = table.Column<string>(type: "text", nullable: true),
                    result = table.Column<string>(type: "text", nullable: true),
                    result_detail = table.Column<string>(type: "text", nullable: true),
                    city_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_details", x => x.user_id);
                });

            migrationBuilder.CreateTable(
                name: "user_extra_datas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    readed_news = table.Column<List<Guid>>(type: "uuid[]", nullable: true),
                    closed_popup = table.Column<List<Guid>>(type: "uuid[]", nullable: true),
                    language = table.Column<string>(type: "text", nullable: true),
                    settings = table.Column<Dictionary<string, string>>(type: "jsonb", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_extra_datas", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    phone_number = table.Column<string>(type: "text", nullable: true),
                    service_id = table.Column<int>(type: "integer", nullable: false),
                    service_buy_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    service_valid_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    user_type = table.Column<int>(type: "integer", nullable: false),
                    register_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    pass = table.Column<string>(type: "text", nullable: true),
                    examiner_finder = table.Column<Guid>(type: "uuid", nullable: true),
                    display_name = table.Column<string>(type: "text", nullable: true),
                    first_and_last_name = table.Column<string>(type: "text", nullable: true),
                    disable_zarin_pal_buying = table.Column<bool>(type: "boolean", nullable: false),
                    last_sms_send = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    avatar = table.Column<string>(type: "text", nullable: true),
                    email = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "verification_codes",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    phone_number = table.Column<string>(type: "text", nullable: true),
                    code = table.Column<int>(type: "integer", nullable: false),
                    market_id = table.Column<int>(type: "integer", nullable: false),
                    metrix_session_id = table.Column<string>(type: "text", nullable: true),
                    advertising_id = table.Column<string>(type: "text", nullable: true),
                    metrix_user_id = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_verification_codes", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "admin_api_calls",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    admin_id = table.Column<Guid>(type: "uuid", nullable: false),
                    api_name = table.Column<string>(type: "text", nullable: true),
                    call_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    data = table.Column<JToken>(type: "jsonb", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_admin_api_calls", x => x.id);
                    table.ForeignKey(
                        name: "fk_admin_api_calls_admin_users_admin_id",
                        column: x => x.admin_id,
                        principalTable: "admin_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "data_source_json_convertors",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    cloned_from_id = table.Column<Guid>(type: "uuid", nullable: false),
                    convertor = table.Column<JToken>(type: "jsonb", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_data_source_json_convertors", x => x.id);
                    table.ForeignKey(
                        name: "fk_data_source_json_convertors_data_source_json_pairs_cloned_f",
                        column: x => x.cloned_from_id,
                        principalTable: "data_source_json_pairs",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_examiners",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    examiner_id = table.Column<Guid>(type: "uuid", nullable: false),
                    accepted = table.Column<bool>(type: "boolean", nullable: false),
                    is_removed = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_examiners", x => x.id);
                    table.ForeignKey(
                        name: "fk_user_examiners_users_examiner_id",
                        column: x => x.examiner_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_user_examiners_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_inbox_items",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    customer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    notif_type = table.Column<int>(type: "integer", nullable: false),
                    title = table.Column<string>(type: "text", nullable: false),
                    content = table.Column<string>(type: "text", nullable: false),
                    files = table.Column<List<string>>(type: "text[]", nullable: true),
                    on_arive_method = table.Column<int>(type: "integer", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    start_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    is_removed = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_inbox_items", x => x.id);
                    table.ForeignKey(
                        name: "fk_user_inbox_items_users_customer_id",
                        column: x => x.customer_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "users_migrate_data",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    vrsion = table.Column<string>(type: "text", nullable: true),
                    enitity_name = table.Column<string>(type: "text", nullable: true),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_users_migrate_data", x => x.id);
                    table.ForeignKey(
                        name: "fk_users_migrate_data_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_admin_api_calls_admin_id",
                table: "admin_api_calls",
                column: "admin_id");

            migrationBuilder.CreateIndex(
                name: "ix_data_source_json_convertors_cloned_from_id",
                table: "data_source_json_convertors",
                column: "cloned_from_id");

            migrationBuilder.CreateIndex(
                name: "ix_data_source_json_pairs_cloned_from_id",
                table: "data_source_json_pairs",
                column: "cloned_from_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_examiners_examiner_id",
                table: "user_examiners",
                column: "examiner_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_examiners_user_id",
                table: "user_examiners",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_inbox_items_customer_id",
                table: "user_inbox_items",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "ix_users_migrate_data_user_id",
                table: "users_migrate_data",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "admin_api_calls");

            migrationBuilder.DropTable(
                name: "data_source_json_convertors");

            migrationBuilder.DropTable(
                name: "history");

            migrationBuilder.DropTable(
                name: "history2");

            migrationBuilder.DropTable(
                name: "news");

            migrationBuilder.DropTable(
                name: "payments");

            migrationBuilder.DropTable(
                name: "profiles");

            migrationBuilder.DropTable(
                name: "send_sms_histories");

            migrationBuilder.DropTable(
                name: "transactions");

            migrationBuilder.DropTable(
                name: "user_details");

            migrationBuilder.DropTable(
                name: "user_examiners");

            migrationBuilder.DropTable(
                name: "user_extra_datas");

            migrationBuilder.DropTable(
                name: "user_inbox_items");

            migrationBuilder.DropTable(
                name: "users_migrate_data");

            migrationBuilder.DropTable(
                name: "verification_codes");

            migrationBuilder.DropTable(
                name: "admin_users");

            migrationBuilder.DropTable(
                name: "data_source_json_pairs");

            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
