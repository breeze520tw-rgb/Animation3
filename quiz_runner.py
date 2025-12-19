import csv
import os
import tkinter as tk
from tkinter import messagebox

class QuizApp:
    def __init__(self, root):
        self.root = root
        self.root.title("期末測驗")
        self.root.geometry("1200x400")
        
        self.csv_path = r"c:\Users\user\OneDrive\桌面\程式設計\5動畫期末\quiz.csv"
        self.questions = []
        self.current_index = 0
        
        self.load_questions()
        self.setup_ui()
        self.load_current_question()

    def load_questions(self):
        if not os.path.exists(self.csv_path):
            messagebox.showerror("錯誤", f"找不到檔案: {self.csv_path}")
            self.root.destroy()
            return

        try:
            with open(self.csv_path, mode='r', encoding='utf-8-sig') as f:
                reader = csv.DictReader(f)
                self.questions = list(reader)
            if not self.questions:
                messagebox.showerror("錯誤", "CSV 檔案是空的！")
                self.root.destroy()
        except Exception as e:
            messagebox.showerror("錯誤", f"讀取 CSV 失敗: {e}")
            self.root.destroy()

    def setup_ui(self):
        self.lbl_question = tk.Label(self.root, text="", font=("微軟正黑體", 14), wraplength=1100)
        self.lbl_question.pack(pady=20, fill='x', padx=20)
        
        # 直接設定輸入框寬度為 80 (約 800 像素)，這就是您要的「加寬」
        self.entry_answer = tk.Entry(self.root, font=("微軟正黑體", 12), width=110)
        self.entry_answer.pack(pady=10)
        # 綁定 Enter 鍵，按下 Enter 等同於按下送出
        self.entry_answer.bind('<Return>', self.check_answer)
        
        self.lbl_feedback = tk.Label(self.root, text="", font=("微軟正黑體", 12), fg="blue")
        self.lbl_feedback.pack(pady=10)
        
        # 建立一個按鈕區域，統一管理所有按鈕
        self.frame_buttons = tk.Frame(self.root)
        self.frame_buttons.pack(pady=20, fill='x') # 增加 fill='x' 確保寬度足夠
        
        # 將所有按鈕 (送出、重新作答、下一題) 都放在 frame_buttons 裡面
        # 這裡先不 pack 按鈕，由 load_current_question 決定顯示哪一個
        self.btn_submit = tk.Button(self.frame_buttons, text="送出答案", font=("微軟正黑體", 12), bg="#dddddd", command=self.check_answer)
        self.btn_retry = tk.Button(self.frame_buttons, text="重新作答", font=("微軟正黑體", 12), bg="#ff9999", command=self.retry_action)
        self.btn_next = tk.Button(self.frame_buttons, text="下一題", font=("微軟正黑體", 12), bg="#99ff99", command=self.next_action)

    def load_current_question(self):
        if self.current_index < len(self.questions):
            q = self.questions[self.current_index]
            self.lbl_question.config(text=f"題目: {q['question']}")
            self.entry_answer.delete(0, tk.END)
            self.lbl_feedback.config(text="")
            
            # 重置按鈕狀態：只顯示「送出答案」
            self.btn_retry.pack_forget()
            self.btn_next.pack_forget()
            self.btn_submit.pack(side=tk.TOP, padx=10, ipadx=10) # 使用 TOP 或 LEFT 都可以，這裡確保它出現
            
            # 讓輸入框自動取得焦點
            self.entry_answer.focus_set()
        else:
            messagebox.showinfo("結束", "測驗已結束！")
            self.root.destroy()

    def check_answer(self, event=None):
        user_input = self.entry_answer.get().strip()
        if not user_input: return # 如果沒輸入內容，不動作
        
        correct_answer = self.questions[self.current_index]['answer']
        
        # 先隱藏所有按鈕，避免重複顯示
        self.btn_submit.pack_forget()
        self.btn_retry.pack_forget()
        self.btn_next.pack_forget()

        if user_input == correct_answer:
            self.lbl_feedback.config(text=f"系統: {self.questions[self.current_index]['correct_feedback']}", fg="green")
            self.btn_next.pack(side=tk.LEFT, padx=20, expand=True)
        else:
            self.lbl_feedback.config(text=f"系統: {self.questions[self.current_index]['wrong_feedback']}\n提示: {self.questions[self.current_index]['hint']}", fg="red")
            self.btn_retry.pack(side=tk.LEFT, padx=20, expand=True)
            self.btn_next.pack(side=tk.LEFT, padx=20, expand=True)

    def retry_action(self):
        self.lbl_feedback.config(text="")
        self.entry_answer.delete(0, tk.END)
        
        self.btn_retry.pack_forget()
        self.btn_next.pack_forget()
        self.btn_submit.pack(side=tk.TOP, padx=10, ipadx=10)
        
        self.entry_answer.focus_set()

    def next_action(self):
        self.current_index += 1
        self.load_current_question()

def run_quiz():
    root = tk.Tk()
    app = QuizApp(root)
    root.mainloop()

if __name__ == "__main__":
    run_quiz()