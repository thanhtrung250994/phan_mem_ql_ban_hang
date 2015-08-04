using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.OleDb;
using System.Diagnostics;
using System.Timers;

namespace Quanlicuahanggiaydep
{
    public partial class Main_form : Form
    {
        /// <summary>
        /// list lưu mã sản phẩm truy suất từ cơ sở dữ liệu
        /// </summary>
        public List<string> list_ma_sp = new List<string>();
        /// <summary>
        /// dictionary mã nhân viên &họ tên
        /// </summary>
        public Dictionary<string, string> dictionary_ma_nv = new Dictionary<string, string>();
        /// <summary>
        /// Dictionary lưu mã sản phẩm và giá của sản phẩm đó
        /// </summary>
        public Dictionary<string, Int32> dictionary_sp = new Dictionary<string, Int32>();
        /// <summary>
        /// mã sản phẩm và số lượng hàng còn lại
        /// </summary>
        public Dictionary<string, Int32> dictionary_sp_soluong_before = new Dictionary<string, int>();
        /// <summary>
        /// mã sản phẩm và số lượng san phẩm sau khi in hóa đơn
        /// </summary>
        public Dictionary<string, Int32> dictionary_sp_soluong_after = new Dictionary<string, int>();
        /// <summary>
        /// Mã sản phẩm và số lượng bán được
        /// </summary>
        public Dictionary<string, Int32> dictionary_sp_banchay = new Dictionary<string, int>();
        /// <summary>
        /// Đối tượng kết nối tới cớ sở dữ liệu
        /// </summary>
        OleDbConnection con = new OleDbConnection();

        public Main_form()
        {
            InitializeComponent();
        }

        /// <summary>
        /// biến public dùng để lưu mã số nhân viên sau khi đăng nhập
        /// </summary>
        public string Ma_nv_login = string.Empty;
        /// <summary>
        /// Biến lưu doanh thu trong CSDL của nhân viên để cộng dồn với số lượng bán được trong phiên làm việc và cập nhật sau khi có hóa đơn được tạo
        /// </summary>
        public int doanh_thu_cua_nv_dang_dang_nhap;

        private void Load_form(object sender, EventArgs e)
        {
            lb_ql_banhang_date.Text = DateTime.Now.Day.ToString() + "/" + DateTime.Now.Month.ToString() + "/" + DateTime.Now.Year.ToString();
            lb_MSNV.Text = Ma_nv_login;
            lb_ql_khachhang_msnv.Text = Ma_nv_login;
            lb_ql_nhanvien_msnv.Text = Ma_nv_login;
            lb_ql_taichinh_msnv.Text = Ma_nv_login;
            try
            {
                con.ConnectionString = @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=Quan_ly_ban_hang_giay_dep.accdb";
                con.Open();
                Debug.WriteLine("Connect successful");
                OleDbCommand command = new OleDbCommand();
                command.Connection = con;
                command.CommandText = "select * from thong_tin_sp";
                OleDbDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    list_ma_sp.Add(reader.GetString(0));
                    dictionary_sp.Add(reader.GetString(0), reader.GetInt32(6));
                    dictionary_sp_soluong_after.Add(reader.GetString(0), reader.GetInt32(2));
                }
                con.Close();
                con.Open();
                //[BUG]
                command.CommandText = "select * from Nhan_vien";
                reader = command.ExecuteReader();

                while (reader.Read())
                {
                    dictionary_ma_nv.Add(reader.GetString(0), reader.GetString(6));
                }
                con.Close();
            }
            catch
            {
                Debug.WriteLine("Connect error");
            }
            cb_ql_banhang_masanpham.DataSource = list_ma_sp;
            cb_ql_banhang_masanpham.Text = "";
            dataGridView_banhang.RowHeadersVisible = false;
            dataGridView_khachhang.RowHeadersVisible = false;
            dataGridView_nhanvien.RowHeadersVisible = false;
            dataGridView_taichinh_doanhthunv.RowHeadersVisible = false;
        }

        private void btn_ql_nv_Click(object sender, EventArgs e)
        {
            dataGridView_nhanvien.Rows.Clear();

            OleDbCommand command = new OleDbCommand();
            command.Connection = con;
            con.Open();
            //chỉ cần kiểm tra xem ô nhập giới tính có trống không, nếu trông
            if (cb_ql_nhanvien_gioitinh.Text != "")
                command.CommandText = "select * from Nhan_vien where Ho_ten LIKE'%" + tb_ql_nv_ten.Text + "%' and Dia_chi LIKE'%" + tb_ql_nhanvien_quequan.Text + "%'and Gioi_tinh='" + cb_ql_nhanvien_gioitinh.Text + "'";
            else
                command.CommandText = "select * from Nhan_vien where Ho_ten LIKE'%" + tb_ql_nv_ten.Text + "%' and Dia_chi LIKE'%" + tb_ql_nhanvien_quequan.Text + "%' ";

            OleDbDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                dataGridView_nhanvien.Rows.Add(reader.GetString(0).ToString(),
                    reader.GetString(6).ToString(),
                    reader.GetString(7).ToString(),
                    reader.GetDateTime(1).ToString(),
                    reader.GetString(2).ToString(),
                    reader.GetDateTime(4).ToString());
            }
            con.Close();
            //dataGridView1.DataSource = table;
            cb_ql_nhanvien_gioitinh.Text = "";
            tb_ql_nhanvien_quequan.Text = "";
            tb_ql_nv_ten.Text = "";
        }

        private void btn_ql_khachhang_search_Click(object sender, EventArgs e)
        {
            dataGridView_khachhang.Rows.Clear();

            OleDbCommand command = new OleDbCommand();
            command.Connection = con;
            con.Open();
            command.CommandText = "select * from Khach_hang where Ten_khachhang LIKE '%" + tb_ql_khachhang_ten.Text + "%' and Ma_khachhang LIKE '%" + tb_ql_khachhang_makhachhang.Text + "%'";
            OleDbDataReader reader = command.ExecuteReader();
            try
            {
                while (reader.Read())
                {
                    if (tb_ql_khachhang_diem.Text != "")
                    {
                        if (reader.GetInt32(2) >= Int32.Parse(tb_ql_khachhang_diem.Text))
                        {
                            dataGridView_khachhang.Rows.Add(reader.GetString(0).ToString(), reader.GetString(1).ToString(), reader.GetInt32(2).ToString());
                        }
                    }
                    else
                    {
                        dataGridView_khachhang.Rows.Add(reader.GetString(0).ToString(), reader.GetString(1).ToString(), reader.GetInt32(2).ToString());
                    }
                }
            }
            catch (Exception qw)
            {
                MessageBox.Show(qw.ToString());
            }
            con.Close();
            tb_ql_khachhang_diem.Text = "";
            tb_ql_khachhang_makhachhang.Text = "";
            tb_ql_khachhang_diem.Text = "";
        }

        /// <summary>
        /// biến lưu lại tên khách hàng sau khi so sánh chuỗi nhập vào từ textbox "Tên/mã khách hàng"
        /// </summary>
        public string ten_khach_hang;
        /// <summary>
        /// biến lưu lại diemr thưởng của khách hàng khi chưa làm hóa đơn
        /// </summary>
        public Int32 Diem_khach_hang;
        /// <summary>
        /// Biến lưu lại mã khách hàng sau khi nhập vao ô Tên/mã khách hàng. Mã này có thể chính là nội dung của textbox, cũng có thể đc sinh ra do n dung nhập vào không phải là mã khách hàng
        /// </summary>
        public string MKH;
        /// <summary>
        /// khi hoàn thành nhập liệu ở textbox "Tên/mã khách hàng" sẽ đem so sánh xem đó là tên mới hay là mã khách hàng đã có trong cơ sở dữ liệu
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tb_ql_banhang_tenkhachhang_Leave(object sender, EventArgs e)
        {

            try
            {
                if (tb_ql_banhang_tenkhachhang.Text != "")
                {
                    OleDbCommand command = new OleDbCommand();
                    command.Connection = con;
                    con.Open();
                    command.CommandText = "select * from Khach_hang where Ma_khachhang = '" + tb_ql_banhang_tenkhachhang.Text + "'";
                    OleDbDataReader reader = command.ExecuteReader();
                    int count = 0;
                    while (reader.Read())
                    {
                        count++;
                        ten_khach_hang = reader.GetString(1);
                        Diem_khach_hang = reader.GetInt32(2);
                    }
                    if (count == 1)
                    {
                        if (MessageBox.Show("Bạn vừa nhập vào mã khách hàng \"" + tb_ql_banhang_tenkhachhang.Text + "\"\ncó tên là \"" + ten_khach_hang + "\"", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                            tb_ql_banhang_tenkhachhang.Text = "";
                        else
                            if (tb_ql_banhang_soluong.Text != "" && cb_ql_banhang_masanpham.Text != "")
                                tb_ql_banhang_thanhtien.Text = (dictionary_sp.First(u => u.Key == cb_ql_banhang_masanpham.Text).Value * Int32.Parse(tb_ql_banhang_soluong.Text)).ToString();
                        //else
                        MKH = tb_ql_banhang_tenkhachhang.Text;
                    }
                    else
                    {
                        MKH = "KH" + DateTime.Now.Second.ToString() + DateTime.Now.Minute.ToString() + DateTime.Now.Hour.ToString() + DateTime.Now.Day.ToString() + DateTime.Now.Month.ToString() + DateTime.Now.Year.ToString();
                        if (MessageBox.Show("Bạn có muốn tạo tên khách hàng mới là: \"" + tb_ql_banhang_tenkhachhang.Text + "\" với mã khách hàng là \"" + MKH + "\"", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                            tb_ql_banhang_tenkhachhang.Text = "";
                        else
                            if (tb_ql_banhang_soluong.Text != "" && cb_ql_banhang_masanpham.Text != "")
                                tb_ql_banhang_thanhtien.Text = (dictionary_sp.First(u => u.Key == cb_ql_banhang_masanpham.Text).Value * Int32.Parse(tb_ql_banhang_soluong.Text)).ToString();

                    }
                    con.Close();
                }
            }
            catch (Exception ea)
            {
                MessageBox.Show(ea.ToString());
            }
        }


        /// <summary>
        /// Khi có sự thay đổi của textbox số lượng sản phẩm
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tb_ql_banhang_soluong_TextChanged(object sender, EventArgs e)
        {
            if (cb_ql_banhang_masanpham.Text == "" || tb_ql_banhang_tenkhachhang.Text == "")
            {
                MessageBox.Show("Bạn hãy nhập đầy đủ \"tên khách hàng\" và \"mã sản phẩm\"");
            }
            else
            {
                if (tb_ql_banhang_soluong.Text == "")
                    tb_ql_banhang_thanhtien.Text = "";
                else
                {
                    try
                    {
                        tb_ql_banhang_thanhtien.Text = (dictionary_sp.First(u => u.Key == cb_ql_banhang_masanpham.Text).Value * Int32.Parse(tb_ql_banhang_soluong.Text)).ToString();

                    }
                    catch { }
                }
            }
        }

        /// <summary>
        /// Tổng giá trị hóa đơn
        /// </summary>
        public Int32 sum;
        /// <summary>
        /// sự kiện click xảy ra thì ghi nhận các thông tin tạm vào đơn hàng
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_ql_banhang_ok_Click(object sender, EventArgs e)
        {
            if (tb_ql_banhang_soluong.Text != ""&&tb_ql_banhang_tenkhachhang.Text!=""&&cb_ql_banhang_masanpham.Text!="")
            {
                try
                {
                    if (dataGridView_banhang.RowCount > 2)
                        dataGridView_banhang.Rows.Remove(dataGridView_banhang.Rows[dataGridView_banhang.RowCount - 2]);
                    dataGridView_banhang.Rows.Add(ten_khach_hang, cb_ql_banhang_masanpham.Text, dictionary_sp.First(u => u.Key == cb_ql_banhang_masanpham.Text).Value.ToString(), tb_ql_banhang_soluong.Text, tb_ql_banhang_thanhtien.Text);
                    int i = 0;
                    sum = 0;
                    while (i < dataGridView_banhang.RowCount - 1)
                    {
                        sum += Int32.Parse(dataGridView_banhang.Rows[i].Cells["thanhtien"].Value.ToString());
                        i++;
                    }
                }
                catch (Exception es)
                {
                    MessageBox.Show(es.ToString());
                }
                string a;
                a = "Tổng: " + sum;
                dataGridView_banhang.Rows.Add("", "", "", "", a);
            }
            else
            {
                MessageBox.Show("Vui lòng điền đầy đủ các trường thông tin!");
            }

        }

        private void cb_ql_banhang_masanpham_TextChanged(object sender, EventArgs e)
        {
            if (tb_ql_banhang_soluong.Text != "")
            {
                tb_ql_banhang_thanhtien.Text = (dictionary_sp.First(u => u.Key == cb_ql_banhang_masanpham.Text).Value * Int32.Parse(tb_ql_banhang_soluong.Text)).ToString();
            }
        }

        private void btn_ql_banhang_huy_Click(object sender, EventArgs e)
        {
            dataGridView_banhang.Rows.Clear();
            tb_ql_banhang_soluong.Text = "";
            tb_ql_banhang_tenkhachhang.Text = "";
            cb_ql_banhang_masanpham.Text = "";
            tb_ql_banhang_thanhtien.Text = "";
        }


        private void btn_inhoadon_Click(object sender, EventArgs e)
        {
            if (tb_ql_banhang_tenkhachhang.Text != "" && dataGridView_banhang.RowCount > 2 && tb_ql_banhang_tenkhachhang.Text == MKH)
            {
                Int32 diem = Diem_khach_hang + sum / 100000;
                doanh_thu_cua_nv_dang_dang_nhap += sum;
                int ij=0;
                while(ij<dataGridView_banhang.RowCount-2)
                {
                    dictionary_sp_soluong_after.Add(dataGridView_banhang.Rows[ij].Cells[1].ToString(), (dictionary_sp_soluong_after.First(u => u.Key == dataGridView_banhang.Rows[ij].Cells[1].ToString()).Value) - int.Parse(dataGridView_banhang.Rows[ij].Cells[3].ToString()));
                    ij++;
                }
                OleDbCommand command = new OleDbCommand();
                command.Connection = con;
                con.Open();
                command.CommandText = @"update Khach_hang set Diem=" + diem + " where MA_khachhang='" + MKH + "'";
                command.ExecuteNonQuery();
                command.CommandText = @"UPDATE Nhan_vien SET Doanh_thu=" + doanh_thu_cua_nv_dang_dang_nhap + " where Ma_nv='" + Ma_nv_login + "'";
                command.ExecuteNonQuery();
                foreach(string sp in dictionary_sp_soluong_after.Keys)
                {
                command.CommandText = @"UPDATE thong_tin_sp SET So_luong =" + dictionary_sp_soluong_after.First(u=>u.Key==sp).Value + " where Ma_SP='" + sp + "'";
                command.ExecuteNonQuery();
                }
                int i = 0;
                try
                {
                    while (i < dataGridView_banhang.RowCount - 2)
                    {
                        command.CommandText = @"insert into Ghi_vet_ban_hang (ngay_ban,Ma_nv,Ma_kh,Ma_sp,So_luong) values ('" + lb_ql_banhang_date.Text + "','" + Ma_nv_login + "','" + MKH + "','" + dataGridView_banhang.Rows[i].Cells[1].Value.ToString() + "'," + Int32.Parse(dataGridView_banhang.Rows[i].Cells[3].Value.ToString()) + ")";
                        i++;
                        command.ExecuteNonQuery();
                    }
                }
                catch(Exception exa)
                {
                    MessageBox.Show(exa.ToString());
                }
                con.Close();
                dataGridView_banhang.Rows.Clear();
                tb_ql_banhang_tenkhachhang.Text = "";
            }
            if (tb_ql_banhang_tenkhachhang.Text != "" && dataGridView_banhang.RowCount > 2 && tb_ql_banhang_tenkhachhang.Text != MKH)
            {
                Int32 diem = sum / 100000;
                doanh_thu_cua_nv_dang_dang_nhap += sum;
                OleDbCommand command = new OleDbCommand();
                command.Connection = con;
                con.Open();
                command.CommandText = @"insert into Khach_hang (MA_khachhang,Ten_khachhang,Diem) values('" + MKH + "','" + tb_ql_banhang_tenkhachhang.Text + "','" + diem + "')";
                command.ExecuteNonQuery();
                command.CommandText = @"UPDATE Nhan_vien SET Doanh_thu=" + doanh_thu_cua_nv_dang_dang_nhap + " where Ma_nv='" + Ma_nv_login + "'";
                command.ExecuteNonQuery();
                int i = 0;
                while (i < dataGridView_banhang.RowCount - 2)
                {
                    command.CommandText = @"insert into Ghi_vet_ban_hang (ngay_ban,Ma_nv,Ma_kh,Ma_sp,So_luong) values ('" + lb_ql_banhang_date.Text + "','" + Ma_nv_login + "','" + MKH + "','" + dataGridView_banhang.Rows[i].Cells[1].Value.ToString() + "'," + Int32.Parse(dataGridView_banhang.Rows[i].Cells[3].Value.ToString()) + ")";
                    i++;
                    command.ExecuteNonQuery();
                }
                con.Close();
                dataGridView_banhang.Rows.Clear();
                tb_ql_banhang_tenkhachhang.Text = "";
            }
        }

        private void btn_ql_sanpham_search_Click(object sender, EventArgs e)
        {
            dataGridView_sanpham.Rows.Clear();
            OleDbCommand command = new OleDbCommand();
            command.Connection = con;
            con.Open();
            if (cb_ql_sanpham_mau.Text != "")
                command.CommandText = "select * from thong_tin_sp where Hang_sx LIKE'%" + cb_ql_sanpham_hangsx.Text + "%' and Mau_sac = '" + cb_ql_sanpham_mau.Text + "'";
            else
                command.CommandText = "select * from thong_tin_sp where Hang_sx LIKE'%" + cb_ql_sanpham_hangsx.Text + "%' ";
            try
            {
                OleDbDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    if (cb_ql_sanpham_size.Text != "")
                    {
                        if (reader.GetInt32(3).ToString() == cb_ql_sanpham_size.Text)
                            dataGridView_sanpham.Rows.Add(reader.GetString(0).ToString(),
                            reader.GetDateTime(1).ToString(),
                            reader.GetInt32(2).ToString(),//số lượng còn lại
                            reader.GetInt32(3).ToString(),
                            reader.GetString(4).ToString(),
                            reader.GetString(5).ToString(),
                            reader.GetInt32(6).ToString(),
                            reader.GetInt32(7).ToString());
                    }
                    else
                        dataGridView_sanpham.Rows.Add(reader.GetString(0).ToString(),
                            reader.GetDateTime(1).ToString(),
                            reader.GetInt32(2).ToString(),
                            reader.GetInt32(3).ToString(),
                            reader.GetString(4).ToString(),
                            reader.GetString(5).ToString(),
                            reader.GetInt32(6).ToString(),
                            reader.GetInt32(7).ToString());
                }
            }
            catch (Exception es)
            {
                MessageBox.Show(es.ToString());
            } con.Close();
            cb_ql_sanpham_mau.Text = "";
            cb_ql_sanpham_size.Text = "";
            cb_ql_sanpham_hangsx.Text = "";
        }

        private void cb_ql_taichinh_options_TextChanged(object sender, EventArgs e)
        {
            if (cb_ql_taichinh_options.Text == "Nhân viên")
            {
                dataGridView_taichinh_doanhthunv.Visible = true;
                dataGridView_taichinh_loinhuan.Visible = false;
                dataGridView_taichinh_spbanchay.Visible = false;
            }
            if (cb_ql_taichinh_options.Text == "Sản phẩm bán chạy")
            {
                dataGridView_taichinh_spbanchay.Visible = true;
                dataGridView_taichinh_loinhuan.Visible = false;
                dataGridView_taichinh_doanhthunv.Visible = false;
            }
            if (cb_ql_taichinh_options.Text == "Lợi nhuận")
            {
                dataGridView_taichinh_doanhthunv.Visible = false;
                dataGridView_taichinh_loinhuan.Visible = true;
                dataGridView_taichinh_spbanchay.Visible = false;
            }
            else
                cb_ql_taichinh_options.Text = "";
        }

        private void btn_ql_taichinh_ok_Click(object sender, EventArgs e)
        {
            try
            {
                if (cb_ql_taichinh_timenam.Text != "" && cb_ql_taichinh_timethang.Text != "")
                {
                    //string time="1/8/2015";// = ("/" + cb_ql_taichinh_timethang.Text + "/" + cb_ql_taichinh_timenam.Text).ToString();
                   
                    //OleDbDataReader reader;
                    if (cb_ql_taichinh_options.Text == "Nhân viên")
                    {
                        OleDbCommand command = new OleDbCommand();
                        command.Connection = con;
                        con.Open();
                        dataGridView_taichinh_doanhthunv.Rows.Clear();
                        foreach (string NV in dictionary_ma_nv.Keys)
                        {
                            command.CommandText = "select * from Ghi_vet_ban_hang where Ma_nv = '" + NV + "'";
                            OleDbDataReader reader = command.ExecuteReader();
                            int sum = 0;
                            while (reader.Read())
                            {

                                //Debug.WriteLine(reader.GetString(2)+"++++++++++++++");
                                string[] subdate = reader.GetDateTime(1).ToString().Split('/');
                                //Debug.WriteLine(subdate[1] + "," + subdate[2]);
                                if (subdate[1] == cb_ql_taichinh_timethang.Text && subdate[2].Substring(0, 4) == cb_ql_taichinh_timenam.Text)
                                {
                                    sum += dictionary_sp.First(u => u.Key == reader.GetString(4).ToString()).Value;
                                }
                            }
                            reader.Close();
                            dataGridView_taichinh_doanhthunv.Rows.Add(dictionary_ma_nv.First(u => u.Key == NV).Value, NV, sum, sum / 1.1 * 0.02);
                        }
                        con.Close();

                    }

                    if (cb_ql_taichinh_options.Text == "Sản phẩm bán chạy")
                    {
                        dataGridView_taichinh_spbanchay.Rows.Clear();
                        OleDbCommand command = new OleDbCommand();
                        command.Connection = con;
                        con.Open();
                        foreach (string SP in dictionary_sp.Keys)
                        {
                           
                                command.CommandText = "select * from Ghi_vet_ban_hang where Ma_sp = '" + SP + "'";
                                OleDbDataReader reader = command.ExecuteReader();
                                int sum = 0;
                                while (reader.Read())
                                {
                                    string[] subdate = reader.GetDateTime(1).ToString().Split('/');
                                    if (subdate[1] == cb_ql_taichinh_timethang.Text && subdate[2].Substring(0, 4) == cb_ql_taichinh_timenam.Text)
                                    {
                                        sum += reader.GetInt32(5);
                                    }
                                }
                                Debug.WriteLine("in rarararararar");
                                reader.Close();
                                dictionary_sp_banchay.Add(SP, sum);
                           
                        }
                        con.Close();
                        int i = 0;
                        int[] arr_sp = dictionary_sp_banchay.Values.ToArray();
                        int count = dictionary_sp_banchay.Count();
                        while (i < 5 && i<count)
                        {
                            int max = 0;
                            foreach (int sp in arr_sp)
                            {
                                if (max < sp)
                                    max = sp;
                            }
                            try
                            {
                                if(max!=0)
                                dataGridView_taichinh_spbanchay.Rows.Add(dictionary_sp_banchay.First(u => u.Value == max).Key.ToString(), max);
                            }
                            catch { }
                                for (int j = 0; j < count; j++)
                            {
                                if (arr_sp[j] == max)
                                    arr_sp[j] = 0;
                            }
                            i++;
                        }
                        dictionary_sp_banchay.Clear();
                    }

                    if (cb_ql_taichinh_options.Text == "Lợi nhuận")
                    {
                        OleDbCommand command = new OleDbCommand();
                        command.Connection = con;
                        con.Open();
                        dataGridView_taichinh_loinhuan.Rows.Clear();
                        command.CommandText = "select * from Ghi_vet_ban_hang";
                        OleDbDataReader reader = command.ExecuteReader();
                        int sum = 0;
                        while (reader.Read())
                        {
                            //Debug.WriteLine(reader.GetString(2)+"++++++++++++++");
                            string[] subdate = reader.GetDateTime(1).ToString().Split('/');
                            //Debug.WriteLine(subdate[1] + "," + subdate[2]);
                            if (subdate[1] == cb_ql_taichinh_timethang.Text && subdate[2].Substring(0, 4) == cb_ql_taichinh_timenam.Text)
                            {
                                sum += dictionary_sp.First(u => u.Key == reader.GetString(4).ToString()).Value;
                            }
                        }
                        con.Close();
                        dataGridView_taichinh_loinhuan.Rows.Add(sum, sum / 1.1 * 0.02, sum / 1.1 * 0.08);
                    }
                    else
                    { }
                    con.Close();
                }
                else
                { }
            }
            catch (Exception aa)
            {
                MessageBox.Show(aa.ToString());
            }
        }

        private void Main_form_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = false;
        }
    }
}
